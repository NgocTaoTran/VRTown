using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using UnityEngine;
using VRTown.Model;
using Zenject;

namespace VRTown.Game
{
    public partial class CharacterInstaller : MonoInstaller<GameInstaller>
    {
        [SerializeField] GameObject _prefabCharacter;
        int Count = 10;

        public override void InstallBindings()
        {
            Container.BindFactory<CharacterType, UserMetaData, Character, Character.Factory>()
                .FromPoolableMemoryPool<CharacterType, UserMetaData, Character, Character.Pool>(poolBinder => poolBinder.WithInitialSize(Count)
                .FromComponentInNewPrefab(_prefabCharacter)
                .UnderTransformGroup("Characters"));
        }
    }
}