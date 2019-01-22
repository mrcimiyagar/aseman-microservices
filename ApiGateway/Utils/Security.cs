using System;
using System.Text;
using ApiGateway.DbContexts;
using SharedArea.Entities;
using SharedArea.Utils;

namespace ApiGateway.Utils
{
    public class Security
    {
        private const string KeySource = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string MakeKey64() => MakeKey(64);

        public static string MakeKey8() => MakeKey(8);

        private static string MakeKey(int length)
        {
            var result = new StringBuilder();
            var rnd = new Random();
            for (var counter = 0; counter < length; counter++)
                result.Append(KeySource[rnd.Next(KeySource.Length - 1)]);
            return result.ToString();
        }

        public static Session Authenticate(DatabaseContext context, string authorization)
        {
            var auth = AuthExtracter.Extract(authorization);
            var session = context.Sessions.Find(auth.SessionId);
            if (session != null && session.Token == auth.Token)
                return session;
            return null;
        }
    }
}