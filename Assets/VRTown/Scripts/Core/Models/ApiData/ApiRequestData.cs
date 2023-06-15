using System;
using System.Collections.Generic;
using System.Linq;

namespace VRTown.Model
{
    public class ApiRequest
    {
        public abstract class BaseApiRequest
        {
            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                Type type = GetType();

                foreach (var f in type.GetFields().Where(f => f.IsPublic))
                    dict.Add(f.Name, f.GetValue(this).ToString());

                return dict;
            }
        }

        public class GetNonceParam : BaseApiRequest
        {
            public string address;
        }
    }
}