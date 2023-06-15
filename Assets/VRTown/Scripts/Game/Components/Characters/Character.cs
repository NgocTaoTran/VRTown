using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using TMPro;
using UnityEngine;
using VRTown.Model;
using Zenject;

namespace VRTown.Game
{
    public enum CharacterType
    {
        User,
        Opponent,
        NPC
    }

    public partial class Character : MonoBehaviour, IPoolable<CharacterType, UserMetaData, IMemoryPool>
    {
        [SerializeField] Transform _camRoot;
        [SerializeField] TMP_Text _textName;

        #region Zenject
        [Inject] IModelController _modelController;
        #endregion Zenject

        #region Properties
        public string Name { get; private set; }
        public CharacterType Type { get; private set; }
        public Vector3 Position { get { return Mover.Position; } }
        public GModel Model { get { return _model; } }
        public IMover Mover { get; private set; }
        public IState State { get; private set; }
        #endregion Properties

        #region Locals
        IMemoryPool _pool;
        UserMetaData _characterData;
        GModel _model;
        #endregion Locals

        void Setup()
        {
            _model = _modelController.CreateCharacter(_characterData.user_data);
            _model.transform.SetParent(this.transform);
            _model.transform.ResetTransform();

            Name = !string.IsNullOrEmpty(_characterData.user_data.name) ? _characterData.user_data.name : "Unknow";
            _textName.text = Name.ToString();
            this.gameObject.name = $"[{Name}] - [ID: {_characterData.user_data.id}]";

            switch (Type)
            {
                case CharacterType.User:
                    {
                        ThirdPersonController controller = GetComponent<ThirdPersonController>();
                        if (controller == null)
                            controller = this.gameObject.AddComponent<ThirdPersonController>();
                        controller.GroundLayers = LayerMask.GetMask("Default");

                        var characterController = GetComponent<CharacterController>();
                        characterController.center = new Vector3(0f, 0.93f, 0f);
                        characterController.radius = 0.28f;
                        characterController.height = 1.8f;
                        characterController.transform.position = GameUtils.GetPositonByArray(_characterData.position);
                        this.gameObject.tag = "Player";
                        Mover = this.GetComponent<GUserMover>();
                        break;
                    }
                case CharacterType.Opponent:
                    {
                        Mover = this.gameObject.AddComponent<GOpponentMover>();
                        State = this.gameObject.AddComponent<GOpponentState>();
                        State.AssignAnimationIDs();
                        State.Setup();
                        transform.position = GameUtils.GetPositonByArray(_characterData.position);
                        break;
                    }
                case CharacterType.NPC:
                    {
                        Mover = this.gameObject.AddComponent<GAutoMover>();
                        transform.position = GameUtils.GetPositonByArray(_characterData.position);
                        break;
                    }
            }
            Mover.Setup();
        }

        public void Release()
        {
            _pool.Despawn(this);
        }

        public void UpdateProfile(UserData newData)
        {
            if (_model.Gender != newData.gender)
            {
                _modelController.ChangeSkin(ref _model, newData);
                switch (Type)
                {
                    case CharacterType.User:
                        GetComponent<ThirdPersonController>().AssignAnimationIDs();
                        break;
                    case CharacterType.Opponent:
                        var state = GetComponent<GOpponentState>();
                        state.AssignAnimationIDs();
                        state.Setup();
                        break;
                }
            }
            else
            {
                UpdateEquipment(newData);
            }
        }

        public void UpdateEquipment(UserData newData)
        {
            _modelController.UpdateEquipments(_model, newData);
        }

        public void SetCamera(GCamera camera)
        {
            var controller = GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                camera.FocusTransform(_camRoot);
                controller.SetCamera(camera);
            }
        }

        public void SetPlayController(PlayerController controller)
        {
            if (Type == CharacterType.User)
            {
                var thirdController = this.GetComponent<ThirdPersonController>();
                thirdController.Setup(controller);
            }
        }

        public void SetListener(ICharacterListener listener)
        {
            if (Type == CharacterType.User)
            {
                var thirdController = this.GetComponent<ThirdPersonController>();
                thirdController.SetListener(listener);
            }
        }

        public void Teleport(Vector3 pos)
        {
            Mover.Teleport(pos);
        }

        public void Move(float x, float y, float z, float d, float gx = 0, float gy = 0)
        {
            Mover.Move(x, y, z, d, gx, gy);
        }

        public void Action(StateData stateData)
        {
            State.AssignAnimationIDs();
            State.Setup(stateData);
        }

        public void Teleport(float x, float y, float z, float d, float gx = 0, float gy = 0)
        {
            Mover.Teleport(x, y, z, d, gx, gy);
        }

        public void OnDespawned()
        {
            switch (Type)
            {
                case CharacterType.User:
                    {
                        var mover = GetComponent<GUserMover>();
                        GameObject.Destroy(mover);
                        break;
                    }

                case CharacterType.Opponent:
                    {
                        var mover = GetComponent<GOpponentMover>();
                        GameObject.Destroy(mover);
                        var state = GetComponent<GOpponentState>();
                        GameObject.Destroy(state);
                        break;
                    }

                case CharacterType.NPC:
                    {
                        var mover = GetComponent<GAutoMover>();
                        GameObject.Destroy(mover);
                        break;
                    }
            }
            _modelController.RemoveCharacter(_model);
            _pool = null;
        }

        public void OnSpawned(CharacterType type, UserMetaData data, IMemoryPool pool)
        {
            Type = type;
            _characterData = data;
            _pool = pool;
            Setup();
        }

        public class Factory : PlaceholderFactory<CharacterType, UserMetaData, Character>
        {
            internal Character Create(Model.CharacterType type, UserMetaData data)
            {
                throw new NotImplementedException();
            }
        }

        public class Pool : MonoPoolableMemoryPool<CharacterType, UserMetaData, IMemoryPool, Character> { }
    }
}