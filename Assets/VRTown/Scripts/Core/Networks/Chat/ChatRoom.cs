using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using VRTown.Utilities;
using VRTown.Model;
using System;

namespace VRTown.Network
{
    public abstract class ChatRoom
    {
        public abstract ChannelType Type { get; }
        public PriorityList<MessageData> Messages { get { return _messages; } }

        protected IGameServer _server;
        protected IChannel _channel;

        protected PriorityList<MessageData> _messages = new PriorityList<MessageData>();

        public abstract void Connect();
        public abstract void Leave();
        public abstract void SendMessage(MessageData message);

        protected abstract void OnReceivedChannelPresence(IChannelPresenceEvent presenceEvent);
        protected abstract void OnReceivedChannelMessage(IApiChannelMessage message);

        protected void RegisterListeners()
        {
            _server.Socket.ReceivedChannelPresence += OnReceivedChannelPresence;
            _server.Socket.ReceivedChannelMessage += OnReceivedChannelMessage;
        }

        protected void RemoveListeners()
        {
            _server.Socket.ReceivedChannelPresence -= OnReceivedChannelPresence;
            _server.Socket.ReceivedChannelMessage -= OnReceivedChannelMessage;
        }
    }
}