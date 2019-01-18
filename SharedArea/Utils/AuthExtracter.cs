using System;
using SharedArea.Middles;

namespace SharedArea.Utils
{
    public static class AuthExtracter
    {
        public const string AK = "Authorization";
        
        public static ReqAuth Extract(string header)
        {
            var parts = header.Split(" ");
            return new ReqAuth()
            {
                SessionId = long.Parse(parts[0]),
                Token = parts[1]
            };
        }
    }
}