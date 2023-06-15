#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRTown.Network;

[InitializeOnLoad]
public static class BuildTools
{
    private const string LOCAL_SERVER_DEFINE = "LOCAL_SERVER";
    private const string DEVELOPMENT_SERVER_DEFINE = "DEVELOPMENT_SERVER";
    private const string STAGING_SERVER_DEFINE = "STAGING_SERVER";
    private const string PRODUCTION_SERVER_DEFINE = "PRODUCTION_SERVER";

    private static List<BuildTargetGroup> BuildTargetGroups = new List<BuildTargetGroup>
    {
        BuildTargetGroup.Standalone,
        BuildTargetGroup.Android,
        BuildTargetGroup.iOS,
        BuildTargetGroup.WebGL
    };

    private static List<GameEnvironment> BuildTargetHosts = new List<GameEnvironment>
    {
        // GameEnvironment.Local,
        GameEnvironment.Development,
        GameEnvironment.Staging,
        GameEnvironment.Production
    };

    [InitializeOnLoadMethod]
    public static void Initalize()
    {
        EditorApplication.delayCall += () =>
        {
            Debug.LogWarning($"[GAME] Target Environment <b>{EnvironmentManager.Environment}</b>");
            InitializeTargetServerOptions();
        };
    }

    private static void InitializeTargetServerOptions()
    {
        // mark the corresponding menu item with a checkmark
        // Menu.SetChecked("VRTown/Target Server/Local", EnvironmentManager.Environment == GameEnvironment.Local);
        Menu.SetChecked("VRTown/Target Server/Development", EnvironmentManager.Environment == GameEnvironment.Development);
        Menu.SetChecked("VRTown/Target Server/Staging", EnvironmentManager.Environment == GameEnvironment.Staging);
        Menu.SetChecked("VRTown/Target Server/Production", EnvironmentManager.Environment == GameEnvironment.Production);
    }

    // [MenuItem("VRTown/Target Server/Local", priority = 0)]
    // public static void SetTargetServerLocal()
    // {
    //     SetTargetServer(GameEnvironment.Local);
    //     SetProfileAddressable(AddressableEnvironment.Development);
    // }

    [MenuItem("VRTown/Target Server/Development", priority = 1)]
    public static void SetTargetServerDevelopment()
    {
        SetTargetServer(GameEnvironment.Development);
        // SetProfileAddressable(AddressableEnvironment.Development);
    }

    [MenuItem("VRTown/Target Server/Staging", priority = 2)]
    public static void SetTargetServerStaging()
    {
        SetTargetServer(GameEnvironment.Staging);
        // SetProfileAddressable(AddressableEnvironment.Production);
    }

    [MenuItem("VRTown/Target Server/Production", priority = 3)]
    public static void SetTargetServerProduction()
    {
        SetTargetServer(GameEnvironment.Production);
        // SetProfileAddressable(AddressableEnvironment.Production);
    }

    private static void SetTargetServer(GameEnvironment targetHost)
    {
        Debug.LogWarningFormat("Updating the target server to <b>{0}</b>...", targetHost);

        BuildTargetGroups.ForEach(targetGroup =>
        {
            // get defines for this build target
            var buildTargetDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var buildTargetDefinesList = new List<string>(buildTargetDefines.Split(';'));

            // check whether we need to add or remove constants
            BuildTargetHosts.ForEach(host =>
            {
                var constantValue = GetDefineConstantForTargetHost(host);
                if (constantValue == null)
                    return;

                if (buildTargetDefines.Contains(constantValue) && host != targetHost)
                {
                    // constant value is present, but not the intended target; remove it
                    buildTargetDefinesList.RemoveAll(i => i.Equals(constantValue, StringComparison.InvariantCultureIgnoreCase));
                    //Debug.LogWarningFormat("Removing <b>{0}</b> from scripting defines for {1}", constantValue, targetGroup);
                }
                else if (!buildTargetDefines.Contains(constantValue) && host == targetHost)
                {
                    // constant value is not present, but is the intended target; add it
                    buildTargetDefinesList.Add(constantValue);
                    //Debug.LogWarningFormat("Adding <b>{0}</b> to scripting defines for {1}", constantValue, targetGroup);
                }
            });

            var newBuildTargetDefines = string.Join(";", buildTargetDefinesList.ToArray());
            if (newBuildTargetDefines != buildTargetDefines)
            {
                //Debug.LogWarningFormat("Setting scripting defines for {0} to: {1}", targetGroup, newBuildTargetDefines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newBuildTargetDefines);
            }

            InitializeTargetServerOptions();
        });
    }

    public static string GetDefineConstantForTargetHost(GameEnvironment targetHost)
    {
        switch (targetHost)
        {
            // case GameEnvironment.Local:
            //     return LOCAL_SERVER_DEFINE;

            case GameEnvironment.Development:
                return DEVELOPMENT_SERVER_DEFINE;

            case GameEnvironment.Staging:
                return STAGING_SERVER_DEFINE;

            case GameEnvironment.Production:
                return PRODUCTION_SERVER_DEFINE;
            default:
                return null;
        }
    }

    // static string GetAddressableProfileName(AddressableEnvironment addressable)
    // {
    //     switch (addressable)
    //     {
    //         case AddressableEnvironment.Development:
    //             return "Development";
    //         default:
    //             return "Production";
    //     }
    // }

    // static void SetProfileAddressable(AddressableEnvironment environment)
    // {
    //     AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
    //     AddressableAssetProfileSettings profile = settings.profileSettings;
    //     string profileID = settings.profileSettings.GetProfileId(GetAddressableProfileName(environment));
    //     settings.activeProfileId = profileID;
    // }
}
#endif