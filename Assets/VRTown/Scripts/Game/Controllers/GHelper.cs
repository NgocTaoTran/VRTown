using UnityEngine.InputSystem;
using VRTown.Service;
using Zenject;

namespace VRTown.Game
{
    public static class GHelper
    {
        public static string UserNID;
        public static ModelController Model;
        public static ConfigController Config;
        public static PlayerInput GameInput;
        public static LocalizationManager Localization;
        public static SignalBus Signal;
    }
}