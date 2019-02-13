using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using ApiGateway.DbContexts;
using ApiGateway.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedArea.Middles;
using SharedArea.Utils;
using Notification = SharedArea.Notifications.Notification;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotifController : Controller
    {
        [Route("~/api/notif/notify_notif_received")]
        [HttpPost]
        public ActionResult<Packet> NotifyNotifReceived([FromBody] Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};
                
                dbContext.Entry(session).Collection(s => s.Notifications).Load();
                
                var notif = session.Notifications.Find(n => n.NotificationId == packet.Notif.NotificationId);
                if (notif == null) return new Packet() {Status = "error_1"};
                
                session.Notifications.Remove(notif);
                dbContext.SaveChanges();
                
                Startup.Pusher.NextPush(session.SessionId);
            }

            return new Packet() {Status = "success"};
        }

        [Route("~/api/notif/get_notifs")]
        [HttpGet]
        public ActionResult<List<Notification>> GetNotifs()
        {
            using (var dbContext = new DatabaseContext())
            {
                return dbContext.Notifications.Include(n => n.Session).ThenInclude(s => s.BaseUser).ToList();
            }
        }
    }
}