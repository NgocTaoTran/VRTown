using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;
using VRTown.Game;
using VRTown.Model;

namespace VRTown.Service
{
    public interface IUserManager
    {
        string UserID { get; set; }
        string UserName { get; }
        UserData Profile { get; set; }
        bool ForceUpdate { get; }
        UniTask Initialize();
        void SetDisplayName(string newName);
        void LoadProfile(IApiAccount account);
        void CreateRandom(string uid);
    }

    public class UserManager : IUserManager
    {
        #region Properties
        public string UserID { get { return _profile.id; } set { _userID = value; } }
        public string UserName { get { return _profile.name; } set { _userName = value; } }
        public UserData Profile { get { return _profile; } set { _profile = value; } }
        public bool ForceUpdate { get { return _forceUpdate; } }
        #endregion Properties

        #region Local
        string _userID;
        string _userName;
        UserData _profile;
        bool _forceUpdate = false;
        #endregion Local

        public UniTask Initialize()
        {
            return default;
        }

        public void CreateRandom(string uid)
        {
            _forceUpdate = true;
            _userID = uid;
            _profile = new UserData(uid);
            _profile.gender = (Random.Range(0, 100) % 2 == 0) ? GenderType.male : GenderType.female;
            _profile.skin = Random.Range(0, C.ColorConfig.Colors.Length);

            _profile.equipments = new List<EquipmentData>();
            foreach (PropType proType in GHelper.Config.DefaultProps)
            {
                var propTypes = GHelper.Config.GetProps(_profile.gender, proType);
                if (propTypes != null && propTypes.Count > 0)
                {
                    var ranItem = propTypes[Random.Range(0, propTypes.Count)];
                    _profile.equipments.Add(new EquipmentData()
                    {
                        type = ranItem.type,
                        id = ranItem.id,
                        color = Random.Range(0, C.ColorConfig.Colors.Length)
                    });
                }
            }
        }

        public void LoadProfile(IApiAccount account)
        {
            _profile = JsonConvert.DeserializeObject<UserMetaData>(account.User.Metadata).user_data;
            if (_profile == null)
            {
                CreateRandom(_userID);
            }
        }

        public void SetDisplayName(string newName)
        {
            _profile.name = newName;
        }
    }
}