using System.Collections.Generic;

namespace VRTown.Model
{
    public class ApiResponse
    {
        public interface IResponse
        { }

        public class NonceData
        {
            public string check;
        }

        public class GetNonceResponse : IResponse
        {
            public NonceData data;
        }

        
    }
}