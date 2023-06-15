using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nakama;
using UnityEngine;
using VRTown.Model;
using VRTown.Utilities;

namespace VRTown.Network
{
    public interface IGameServer
    {
        VRTown.Service.ILogger Logger { get; }
        ServerStatus Status { get; }
        ISocket Socket { get; }
        string UserNID { get; set; }
        public string MatchID { get; }
        bool IsNewProfile { get; }
        OpCodes Type { get; set; }
        IApiAccount Account { get; }
        IServerListener Listener { get; }

        UniTask Authenticate(IServerListener listener, string userId, System.Action<bool> cb = null);
        UniTask ConnectSocket(System.Action cb = null);
        UniTask<IApiRpc> RequestRPC(string rpcName, string payload = "{}");
        UniTask OnSendMatchStateAsync(string matchId, long opCode, string state, IEnumerable<IUserPresence> presences = null);
        UniTask UpdateAccount(UserMetaData metadata);
        UniTask LeaveMatch();
        UniTask JoinMatch();
        UniTask FindMatch(string nameMatch, System.Action cb = null);


        // Chat
        void SendMessage(ChatChannel type, MessageData message);
    }

    public interface IServerListener
    {
        void OnAccountLoaded(IApiAccount account);
        void OnSocketConnected(UserMetaData userData);
        void OnReceivedStreamState(IStreamState streamState);
        void OnReceivedMatchState(IMatchState matchState);
        void OnReceivedStreamPresence(IStreamPresenceEvent presenceEvent);
        void OnMessageChanged(PriorityList<MessageData> messages);
    }
}