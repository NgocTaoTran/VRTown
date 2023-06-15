

using UnityEngine;

namespace VRTown.Model
{
    public static partial class C
    {
        public const string KEY_USER_PROFILE = "USER_PROFILE";

        public static class ColorConfig
        {
            public static string[] Colors = new string[10] { "#B9B9B9", "#DFDFDF", "#ACA29B", "#AC9381", "#AD8365", "#AC7751", "#AD6A3A", "#A85C25", "#8C430F", "#7A3301" };

        }

        public static class TagConfig
        {
            public const string PlayerTag = "Player";
            public const string CharacterTag = "Character";
            public const string Screen = "Screen";
            public const string LiveScreen = "LiveScreen";
            public const string Door = "Door";
            public const string AirBall = "AirBall";
        }

        public static class InputConfig
        {
            public static string NormalMap = "Normal";
            public static string ChatMap = "Chat";
        }

        public static class GameConfig
        {
            public static float SizeLand = 80f;
            public static Vector3 DefaultBegin = Vector3.zero;
        }

        public static class CommunicationConfig
        {
            public static float DistanceScreenNearby = 16f * 6f;
            public static float DistanceChatNearby = 16f * 2f;
        }

        public static class ServerConfig
        {
            public static string NAME_RPC_LOAD_LAND = "get_land_by_region";
            public static string NAME_RPC_GET_MATCH_ID = "find_match";
            public static string NAME_RPC_JOIN = "join";
            public static string NAME_RPC_LOAD_PLAYERS = "list_members";
            public static string NAME_RPC_MOVE = "move";
            public static string NAME_RPC_UPDATE_METADATA = "update_metadata";
            
            public static string NAME_RPC_REGISTER_JOIN_VOICE_AGORA = "join_agora";
            public static string NAME_RPC_GET_NAME_LEAVE_VOICE_AGORA = "leave_agora";
            public static string NAME_RPC_GET_NAME_JOIN_VOICE_AGORA = "get_agora";

            public static string NAME_RPC_REGISTER_JOIN_STREAM_AGORA = "join_stream";
            public static string NAME_RPC_GET_NAME_LEAVE_STREAM_AGORA = "leave_stream";
            public static string NAME_RPC_GET_NAME_JOIN_STREAM_AGORA = "get_stream";
        }

        public static class MapConfig
        {
            public static Vector3 ConvertWorldPosition(Vector3Int mapPos)
            {
                return new Vector3(mapPos.x * 16f, mapPos.z * 16f, mapPos.y * 16f);
            }

            public static Vector3Int ConvertMapPosition(Vector3 worldPos)
            {
                return new Vector3Int(System.Convert.ToInt32(worldPos.x / 16f), System.Convert.ToInt32(worldPos.z / 16f), System.Convert.ToInt32(worldPos.y / 16f));
            }
        }

        public static class LandConfig
        {
            // public static bool Overlap(this LandBorder border, LandBorder otherBorder)
            // {
            //     return border.Border.Overlaps(otherBorder.Border);
            // }
        }
       
    }
}