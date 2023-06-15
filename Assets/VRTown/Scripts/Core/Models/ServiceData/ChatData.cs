

using System;

namespace VRTown.Model
{
    public class MessageData : IComparable<MessageData>
    {
        public string MessageId;
        public string SenderId;
        public string Username;
        public DateTime CreateTime;
        public string Content;

        public MessageData(string mId, string userId, string userName, string content, string createTime)
        {
            MessageId = mId;
            SenderId = userId;
            Username = userName;
            Content = content;
            CreateTime = DateTime.Now;
        }

        public MessageData(string userName, string content)
        {
            Username = userName;
            Content = content;
        }

        public int CompareTo(MessageData obj)
        {
            return CreateTime.CompareTo(obj.CreateTime);
        }
    }
}