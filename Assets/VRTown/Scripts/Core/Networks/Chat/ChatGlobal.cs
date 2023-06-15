using System;
using System.Collections;
using System.Collections.Generic;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;
using VRTown.Game.Data;
using VRTown.Model;
using VRTown.Utilities;

namespace VRTown.Network
{
    public class ChatGlobal : ChatRoom
    {
        public override ChannelType Type { get { return ChannelType.Group; } }
        const string CHANNEL_NAME = "Global";
        List<IUserPresence> _users = new List<IUserPresence>();

        public ChatGlobal(IGameServer server)
        {
            _server = server;
        }

        public override async void Connect()
        {
            RegisterListeners();
            _channel = await _server.Socket.JoinChatAsync(CHANNEL_NAME, ChannelType.Room, true, false);
            _server.Logger.Log("[SERVER] Connect Channel ID: " + _channel.Id);
        }

        public override async void Leave()
        {
            RemoveListeners();
            _server.Logger.Log("[SERVER] Leave Channel ID: " + _channel.Id);
            await _server.Socket.LeaveChatAsync(_channel);
        }

        public override async void SendMessage(MessageData message)
        {
            // var chatData = new Message(_server.Account.User.DisplayName, message.Content);
            Dictionary<string, string> sentData = new Dictionary<string, string>();
            try
            {
                sentData.Add(message.Username, message.Content);
                var result = await _server.Socket.WriteChatMessageAsync(_channel, JsonConvert.SerializeObject(sentData));
                Debug.Log("[SendMessage]: " + JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                Debug.Log("[SERVER] ConnectNakama, Error: " + ex.Message);
            }


        }

        #region Listeners
        protected override void OnReceivedChannelPresence(IChannelPresenceEvent presenceEvent)
        {
            foreach (var presence in presenceEvent.Leaves)
            {
                _users.Remove(presence);
            }
            _users.AddRange(presenceEvent.Joins);
        }

        protected override void OnReceivedChannelMessage(IApiChannelMessage message)
        {
            _messages.Add(new MessageData(message.MessageId, message.SenderId, message.Username, message.Content, message.CreateTime));
            _server.Listener.OnMessageChanged(_messages);
        }
        #endregion Listeners
    }
}