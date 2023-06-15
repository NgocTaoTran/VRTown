namespace VRTown.Network
{
    public enum GameEnvironment
    {
        Development = 0,
        Staging = 1,
        Production = 2
    }

    public static class ServerSchemes
    {
        public const string DEVELOPMENT_SCHEME = "https";
        public const string STAGING_SCHEME = "https";
        public const string PRODUCTION_SCHEME = "https";
    }

    public static class ServerHosts
    {
        public const string DEVELOPMENT_HOST = "dev-rpc-ts-nakama.vrtown.io";
        public const string STAGING_HOST = "uat-rpc-ts-nakama.vrtown.io";
        // public const string STAGING_HOST = "dev-rpc-ts-nakama.vrtown.io";
        public const string PRODUCTION_HOST = "fe-dev-nakama.vrtown.io/";
    }

    public static class ServerPorts
    {
        public const int DEVELOPMENT_PORT = 443;
        public const int STAGING_PORT = 443;
        public const int PRODUCTION_PORT = 443;
    }

    public static class ServerSocketKeys
    {
        public const string DEVELOPMENT_SOCKET_KEY = "Slj71EiZJQwq6dk80QyHhrxMecl4j59E";
        public const string STAGING_SOCKET_KEY = "Kfc2nENC1eYurNoDFKBSvagk2cxvqFow";
        public const string PRODUCTION_SOCKET_KEY = "defaultkey";
    }

    public static class ServerAPIs
    {
        public const string DEVELOPMENT_API_URL = "https://s3.ap-southeast-1.amazonaws.com/cdn.vrtown.io";
        public const string STAGING_API_URL = "https://s3.ap-southeast-1.amazonaws.com/cdn.vrtown.io";
        public const string PRODUCTION_API_URL = "https://s3.ap-southeast-1.amazonaws.com/cdn.vrtown.io";
    }
}