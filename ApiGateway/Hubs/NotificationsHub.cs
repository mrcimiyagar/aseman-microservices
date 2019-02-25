using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
                Startup.Pusher.NextPush(sessionId);
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
                Startup.Pusher.NextPush(sessionId);
                return "success";
            }
        }

        public string KeepAlive()
        {
            using (var dbContext = new DatabaseContext())
            {
                if (dbContext.Sessions.FirstOrDefault(s => s.ConnectionId == Context.ConnectionId) == null)
                {
                    return "failure";
                }
                else
                {
                    return "keep-alive : " + Convert.ToInt64((DateTime.Now - DateTime.MinValue)
                               .TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine(Context.ConnectionId + " client connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine(Context.ConnectionId + " client disconnected .");
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.FirstOrDefault(s => s.ConnectionId == Context.ConnectionId);
                if (session != null)
                {
                    session.ConnectionId = "";
                    session.Online = false;
                    dbContext.SaveChanges();
                }
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}