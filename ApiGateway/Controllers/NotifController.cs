using System.Collections.Generic;
using System.Linq;
using ApiGateway.DbContexts;
using ApiGateway.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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

                using (var mongo = new MongoLayer())
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(packet.Notif.NotificationId));
                    
                    var notif = mongo.GetNotifsColl().Find(filter).FirstOrDefault();
                    if (notif == null) return new Packet() {Status = "error_1"};
                    if (notif["Session"]["SessionId"] == session.SessionId)
                        mongo.GetNotifsColl().DeleteOne(filter);
                }
                
                Startup.Pusher.NotifyNotificationReceived(session.SessionId);
                
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