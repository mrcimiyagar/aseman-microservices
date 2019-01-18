using System;
using System.Globalization;
using Microsoft.AspNetCore.SignalR;

namespace ApiGateway.Hubs
{
    public class NotificationsHub : Hub
    {
        public string Login(long sessionId, string token)
        {
            /*using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, sessionId + " " + token);
                if (session == null) return "error_0";
                session.ConnectionId = Context.ConnectionId;
                session.Online = true;
                context.SaveChanges();
                return "success";
            }*/

            return "not implemented";
        }

        public string LoginBot(long botId, string token)
        {
            /*using (var context = new DatabaseContext())
            {
                var bot = context.Bots.Find(botId);
                if (bot == null) return "error_0";
                context.Entry(bot).Collection(b => b.Sessions).Load();
                var session = bot.Sessions.FirstOrDefault();
                if (session.Token != token) return "error_0";
                session.ConnectionId = Context.ConnectionId;
                session.Online = true;
                context.SaveChanges();
                return "success";
            }*/
            
            return "not implemented";
        }

        public string KeepAlive()
        {
            return "keep-alive : " + Convert.ToInt64((DateTime.Now - DateTime.MinValue)
                           .TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }
    }
}