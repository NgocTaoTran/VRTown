using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using VRTown.Model;
using Newtonsoft.Json;
using System.Threading.Tasks;
using VRTown.Game.Data;

namespace VRTown.Network
{
    public class NakamaServer : IGameServer
    {
        #region Zenject_Binding
        [Inject] VRTown.Service.ILogger _logger;
        #endregion Zenject_Binding

        #region Properties
        public VRTown.Service.ILogger Logger { get { return _logger; } }
        public ServerStatus Status { get { return _status; } }
        public ISocket Socket { get { return _socket; } }
        public string UserNID { get; set; }
        public string MatchID { get { return _matchId; } }
        public bool IsNewProfile { get { return _session.Created; } }
        public OpCodes Type { get { return _type; } set { _type = value; } }   // Created => false: acc da tao, true: moi tao ra
        public IApiAccount Account { get { return _account; } }
        public IServerListener Listener { get { return _serverListener; } }

        #endregion Properties

        #region Local
        ServerStatus _status;
        IClient _client;
        ISocket _socket;
        IMatch _match;
        ISession _session;
        IApiAccount _account;
        List<ChatRoom> _chatRooms = new List<ChatRoom>();
        IServerListener _serverListener = null;
        OpCodes _type;

        string _userId = "";
        string _authToken = "";
        string _refreshToken = "";

        public string _matchId = "";
        #endregion Local

        public async UniTask Authenticate(IServerListener listener, string userId, Action<bool> cb = null)
        {
            _logger.Log("[SERVER] Authenticate: " + userId);
            _userId = userId;
            _serverListener = listener;

            var environment = EnvironmentManager.GetEnvironment();
            _logger.Log("[SERVER] Server Info: " + Newtonsoft.Json.JsonConvert.SerializeObject(environment));

            _client = new Nakama.Client(environment.Scheme, environment.Host, environment.Port, environment.ServerKey, UnityWebRequestAdapter.Instance);
            _client.Timeout = 15;

            await CreateSession();
            await LoadAccount();

            await UniTask.Delay(0.2f);

            cb?.Invoke(true);
        }

        public async UniTask CreateSession()
        {
            _session = null;
            {
                _session = await _client.AuthenticateCustomAsync(_userId);
                _logger.Log("[NAKAMA] UserID = " + _session.UserId);
                UserNID = _session.UserId;
                _authToken = _session.AuthToken;
                _refreshToken = _session.RefreshToken;
            }

            _logger.Log("[NAKAMA] Profile is Created: " + _session.Created);
        }

        public async UniTask ConnectSocket(System.Action cb = null)
        {
            if (_status != ServerStatus.Disconnected) return;
            _status = ServerStatus.Connecting;

            _socket = _client.NewSocket(true);
            _socket.Connected += async () =>
            {
                _serverListener?.OnSocketConnected(Newtonsoft.Json.JsonConvert.DeserializeObject<UserMetaData>(_account.User.Metadata));
                _status = ServerStatus.Connected;
                _logger.Log("[SERVER] On Socket Connected: " + _account.User.Metadata);
                cb?.Invoke();
            };

            _socket.ReceivedStreamState += OnReceivedStreamState;
            _socket.ReceivedStreamPresence += OnReceivedStreamPresence;
            _socket.ReceivedMatchState += OnReceivedMatchState;

            _socket.Closed += () =>
            {
                _socket.ReceivedStreamState -= OnReceivedStreamState;
                _socket.ReceivedStreamPresence -= OnReceivedStreamPresence;
                _status = ServerStatus.Disconnected;
            };

            try
            {
                await _socket.ConnectAsync(_session);
                ConnectChat();
            }
            catch (Exception ex)
            {
                _status = ServerStatus.Disconnected;
                _logger.Log("[SERVER] ConnectNakama, Error: " + ex.Message);
            }

        }

        public async UniTask FindMatch(string nameMatch, System.Action cb = null)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    {"match_name",nameMatch},
                };
                // var result = await _socket.RpcAsync(C.ServerConfig.NAME_RPC_GET_MATCH_ID, "{\"match_name\": \"{vrtown_match}\"}");
                var result = await _socket.RpcAsync(C.ServerConfig.NAME_RPC_GET_MATCH_ID, JsonConvert.SerializeObject(payload));
                _matchId = JsonConvert.DeserializeObject<WorldData>(result.Payload).matchId;
                Debug.Log("[MatchID]: " + result.Payload);
                if (_matchId != null)
                {
                    cb?.Invoke();
                    await JoinMatch();
                }
            }
            catch (Exception ex)
            {
                _logger.Log("[SERVER] Find Match Id, Error: " + ex.Message);
            }
        }

        public async UniTask JoinMatch()
        {
            try
            {
                _status = ServerStatus.JoinMatch;
                _match = await _socket.JoinMatchAsync(_matchId);
            }
            catch (ApiResponseException ex)
            {
                _logger.Log("[SERVER] ConnectNakama - GetAccountAsync, Error: " + ex.Message);
            }
        }

        public async UniTask LeaveMatch()
        {
            try
            {
                await _socket.LeaveMatchAsync(_match);
                _socket.ReceivedMatchState -= OnReceivedMatchState;
                _status = ServerStatus.Leavematch;
            }
            catch (ApiResponseException ex)
            {
                _logger.Log("[SERVER] ConnectNakama - GetAccountAsync, Error: " + ex.Message);
            }
        }


        public async UniTask LoadAccount()
        {
            try
            {
                _account = await _client.GetAccountAsync(_session);
            }
            catch (ApiResponseException ex)
            {
                _logger.Log("[SERVER] ConnectNakama - GetAccountAsync, Error: " + ex.Message);
            }
        }

        public async UniTask UpdateAccount(UserMetaData metadata)
        {
            _logger.Log("[NAKAMA] UpdateAccount_payload" + JsonConvert.SerializeObject(metadata));
            var result = await _socket.RpcAsync(C.ServerConfig.NAME_RPC_UPDATE_METADATA, JsonConvert.SerializeObject(metadata));
            _logger.Log("[NAKAMA] UpdateAccount" + JsonConvert.SerializeObject(result));
        }

        public async UniTask<IApiRpc> RequestRPC(string rpcName, string payload = "{}")
        {
            _logger.Log($"[NAKAMA] RequestRPC [{rpcName}] with payload[{payload}]");
            return await _socket.RpcAsync(rpcName, payload);
        }

        public void ConnectChat()
        {
            var chatGlobal = new ChatGlobal(this);
            chatGlobal.Connect();

            _chatRooms.Add(chatGlobal);
        }

        public void SendMessage(ChatChannel type, MessageData message)
        {
            ChatRoom room = null;
            switch (type)
            {
                case ChatChannel.Global:
                    room = _chatRooms.Find(val => val.Type == ChannelType.Group);
                    break;

                case ChatChannel.Nearby:
                    room = _chatRooms.Find(val => val.Type == ChannelType.Room);
                    break;
            }

            if (room != null)
            {
                room.SendMessage(message);
            }
        }

        public void OnReceivedStreamState(IStreamState streamState)
        {
            // _logger.Log("[NAKAMA] OnReceivedStreamState: " + JsonConvert.SerializeObject(streamState));
            _serverListener?.OnReceivedStreamState(streamState);
        }

        public void OnReceivedStreamPresence(IStreamPresenceEvent presenceEvent)
        {
            // _logger.Log("[NAKAMA] OnReceivedStreamPresence: " + JsonConvert.SerializeObject(presenceEvent));
            _serverListener?.OnReceivedStreamPresence(presenceEvent);
        }

        public async UniTask OnSendMatchStateAsync(string matchId, long opCode, string state, IEnumerable<IUserPresence> presences = null)
        {
            if (_status == ServerStatus.JoinMatch)
                await _socket.SendMatchStateAsync(matchId, opCode, state, presences);
        }

        public void OnReceivedMatchState(IMatchState matchState)
        {
            _serverListener?.OnReceivedMatchState(matchState);
        }
    }
}