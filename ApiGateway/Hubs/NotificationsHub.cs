using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using MessengerPlatform.DbContexts;
using SharedArea.Entities;
using SharedArea.Middles;
using AWP.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AWP.Hubs
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

        public string LoginBot(long botId, string token)
        {
            using (var context = new DatabaseContext())
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
            }
        }

        public string KeepAlive()
        {
            return "keep-alive : " + Convert.ToInt64((DateTime.Now - DateTime.MinValue)
                           .TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }
    }
}