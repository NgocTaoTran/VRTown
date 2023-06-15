using System;
using UnityEngine;
using VRTown.Service;
using VRTown.Model;
using Zenject;
using VRTown.Network;
using VRTown.Game.LuaSystem;
using VRTown.Game.UI;

namespace VRTown.Game
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        public override void InstallBindings()
        {
            Application.targetFrameRate = 60;

            NetworkInstaller.Install(Container);

            InstallServices();
            InstallFactories();
            InstallSignals();
            InstallGame();

            LuaInstaller.Install(Container);
        }

        private void InstallServices()
        {
            Container
                .BindInterfacesTo<WalletController>()
                .AsSingle();

            Container
                .Bind<IBundleLoader>()
                .WithId(BundleLoaderName.Resource)
                .To<ResourceLoader>()
                .AsSingle();

            Container
                .Bind<IBundleLoader>()
                .WithId(BundleLoaderName.Addressable)
                .To<AddressableLoader>()
                .AsSingle();

            Container
                .Bind<IBundleLoader>()
                .WithId(BundleLoaderName.Zip)
                .To<ZipLoader>()
                .AsSingle();

            Container.BindInterfacesTo<UnityLogger>().AsSingle();
            Container.Bind<LocalizationManager>().AsSingle();
            Container.BindInterfacesTo<UserManager>().AsSingle();
            Container.Bind<IGameServer>().To<NakamaServer>().AsSingle();
        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<ForceVoiceSignal>().OptionalSubscriber();
        }

        private void InstallFactories()
        {
            Container
                .BindFactory<IBaseState, BaseState.Factory>()
                .WithId(GameState.Init)
                .To<IGameInit>()
                .FromSubContainerResolve()
                .ByInstaller<GameInit.Installer>();

            Container
                .BindFactory<IBaseState, BaseState.Factory>()
                .WithId(GameState.Login)
                .To<IGameLogin>()
                .FromSubContainerResolve()
                .ByInstaller<GameLogin.Installer>();
            Container
                .BindFactory<IBaseState, BaseState.Factory>()
                .WithId(GameState.GameHome)
                .To<IGameHome>()
                .FromSubContainerResolve()
                .ByInstaller<GameHome.Installer>();
            Container
                .BindFactory<IBaseState, BaseState.Factory>()
                .WithId(GameState.Gameplay)
                .To<IGamePlay>()
                .FromSubContainerResolve()
                .ByInstaller<GamePlay.Installer>();
        }

        private void InstallGame()
        {
            Container.BindInterfacesTo<GameController>().AsSingle();
            Container.BindInterfacesTo<AgoraController>().AsSingle();
            Container.BindInterfacesTo<ModelController>().AsSingle();
            Container.BindInterfacesTo<WorldController>().AsSingle();
            Container.Bind<ConfigController>().AsSingle();
            Container.Bind<LuaInterface>().AsSingle();
        }
    }

    public class NetworkInstaller : Installer<NetworkInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IApiManager>().To<ApiManager>().AsSingle();
        }
    }

    public class LuaInstaller : Installer<LuaInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindFactory<string, GameObject, LuaScript, LuaScript.Factory>().FromPoolableMemoryPool(x => x.WithInitialSize(2));
        }
    }
}