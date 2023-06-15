using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTown.Model
{
    public class ForceVoiceSignal
    {
        public string ChannelID;
        public bool IsTrigerEnter;

        public ForceVoiceSignal(string channelID, bool isTrigerEnter)
        {
            ChannelID = channelID;
            IsTrigerEnter = isTrigerEnter;
        }
    }
}