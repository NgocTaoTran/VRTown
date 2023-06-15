using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Network
{
    public class EnvironmentConfig
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string ServerKey { get; set; }
        public string ApiUrl { get; set; }

        public EnvironmentConfig(string scheme, string host, string apiUrl, int port, string key)
        {
            Scheme = scheme;
            Host = host;
            ApiUrl = apiUrl;
            Port = port;
            ServerKey = key;
        }
    }

    public static class EnvironmentManager
    {
        public const string KEY_GAME_ENVIRONMENT = "GAME_ENVIRONMENT";

        public static GameEnvironment Environment
        {
            get
            {
#if LOCAL_SERVER
			    return GameEnvironment.Local;
#elif DEVELOPMENT_SERVER
                return GameEnvironment.Development;
#elif STAGING_SERVER
                return GameEnvironment.Staging;
#elif PRODUCTION_SERVER
			    return GameEnvironment.Production;
#endif
                return GameEnvironment.Development;
            }
        }

        private static IReadOnlyDictionary<GameEnvironment, EnvironmentConfig> EnvironmentConfigs = new Dictionary<GameEnvironment, EnvironmentConfig>()
        {
            { GameEnvironment.Development, new EnvironmentConfig(ServerSchemes.DEVELOPMENT_SCHEME, ServerHosts.DEVELOPMENT_HOST, ServerAPIs.DEVELOPMENT_API_URL, ServerPorts.DEVELOPMENT_PORT, ServerSocketKeys.DEVELOPMENT_SOCKET_KEY) },
            { GameEnvironment.Staging, new EnvironmentConfig(ServerSchemes.STAGING_SCHEME, ServerHosts.STAGING_HOST, ServerAPIs.STAGING_API_URL, ServerPorts.STAGING_PORT, ServerSocketKeys.STAGING_SOCKET_KEY) },
            { GameEnvironment.Production, new EnvironmentConfig(ServerSchemes.PRODUCTION_SCHEME, ServerHosts.PRODUCTION_HOST, ServerAPIs.PRODUCTION_API_URL, ServerPorts.PRODUCTION_PORT, ServerSocketKeys.PRODUCTION_SOCKET_KEY) }
        };

        public static EnvironmentConfig GetEnvironment() => EnvironmentConfigs[Environment];
    }
}