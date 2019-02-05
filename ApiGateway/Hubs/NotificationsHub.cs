using System;
using System.Globalization;
using System.Linq;
using ApiGateway.DbContexts;
using ApiGateway.Utils;
using Microsoft.AspNetCore.SignalR;

namespace ApiGateway.Hubs
{
    public class NotificationsHub : Hub
    {
        public string Login(long sessionId, string token)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, sessionId + " " + token);
                if (session == null) return "error_0";
                session.ConnectionId = Context.ConnectionId;
                session.Online = true;
                context.SaveChanges();
                return "success";
            }
        }

        public string LoginBot(long sessionId, string token)
        {
            using (var context = new DatabaseContext())
            {
                var session = context.Sessions.Find(sessionId);
                if (session == null) return "error_0";
                if (session.Token != token) return "error_0";
                session.ConnectionId = Context.ConnectionId;
                session.Online = true;
                context.SaveChanges();
                return "success";
            }
        }

        public string KeepAlive()
        {
            return "keep-alive : " + Convert.ToInt64((DateTime.Now - DateTime.MinValue)
                           .TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }
    }
}