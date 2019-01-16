using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MessengerPlatform.DbContexts;
using AWP.Hubs;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using AWP.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public BotController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
        [Route("~/api/robot/get_bots")]
        [HttpPost]
        public ActionResult<Packet> GetBots([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var bots = context.Bots.Include(b => b.BotSecret).ToList();
                var finalBots = new List<Bot>();
                var bannedAccessIds = new List<long>();
                foreach (var bot in bots)
                {
                    if (bot.BotSecret.CreatorId != session.BaseUserId)
                    {
                        bannedAccessIds.Add(bot.BaseUserId);
                    }
                    else
                    {
                        finalBots.Add(bot);
                    }
                }
                using (var nextContext = new DatabaseContext())
                {
                    foreach (var id in bannedAccessIds)
                    {
                        var finalBot = nextContext.Bots.Find(id);
                        finalBots.Add(finalBot);
                    }
                }
                return new Packet {Status = "success", Bots = finalBots};
            }
        }

        [Route("~/api/robot/add_bot_to_room")]
        [HttpPost]
        public ActionResult<Packet> AddBotToRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_3"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_2"};
                context.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_4"};
                context.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == packet.Bot.BaseUserId);
                if (workership != null) return new Packet {Status = "error_1"};
                var bot = context.Bots.Find(packet.Bot.BaseUserId);
                if (bot == null) return new Packet {Status = "error_0"};
                workership = new Workership()
                {
                    BotId = bot.BaseUserId,
                    Room = room,
                    PosX = packet.Workership.PosX,
                    PosY = packet.Workership.PosY,
                    Width = packet.Workership.Width,
                    Height = packet.Workership.Height
                };
                context.AddRange(workership);
                context.SaveChanges();
                Room finalRoom;
                using (var finalContext = new DatabaseContext())
                {
                    finalRoom = finalContext.Rooms.Find(room.RoomId);
                    finalContext.Entry(finalRoom).Reference(r => r.Complex).Load();
                }
                context.Entry(bot).Collection(b => b.Sessions).Load();
                var botSess = bot.Sessions.FirstOrDefault();
                var addition = new BotAdditionToRoomNotification()
                {
                    Room = finalRoom,
                    Session = botSess
                };
                if (botSess.Online)
                {
                    _notifsHub.Clients.Client(botSess.ConnectionId)
                        .SendAsync("NotifyBotAddedToRoom", addition);
                }
                else
                {
                    context.Notifications.Add(addition);
                }
                context.SaveChanges();
                return new Packet
                {
                    Status = "success",
                    Workership = workership
                };
            }
        }

        [Route("~/api/robot/get_bot_store_content")]
        [HttpPost]
        public ActionResult<Packet> GetBotStore()
        {
            using (var context = new DatabaseContext())
            {
                var botStoreHeader = context.BotStoreHeader
                    .Include(bsh => bsh.Banners)
                    .ThenInclude(b => b.Bot)
                    .FirstOrDefault();
                var botStoreSection = new BotStoreSection();
                var botStoreBots = context.Bots.Select(bot => new BotStoreBot()
                    {
                        Bot = bot,
                        BotStoreSection = botStoreSection
                    })
                    .ToList();
                botStoreSection.BotStoreBots = botStoreBots;
                return new Packet
                {
                    Status = "success",
                    BotStoreHeader = botStoreHeader,
                    BotStoreSections = new List<BotStoreSection>() { botStoreSection }
                };
            }
        }

        [Route("~/api/robot/update_workership")]
        [HttpPost]
        public ActionResult<Packet> UpdateWorkership([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_0"};
                context.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_2"};
                context.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == packet.Bot.BaseUserId);
                if (workership == null) return new Packet {Status = "error_3"};
                workership.PosX = packet.Workership.PosX;
                workership.PosY = packet.Workership.PosY;
                workership.Width = packet.Workership.Width;
                workership.Height = packet.Workership.Height;
                context.SaveChanges();
                return new Packet {Status = "success"};
            }
        }

        [Route("~/api/robot/get_created_bots")]
        [HttpPost]
        public ActionResult<Packet> GetCreatedBots()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                var bots = new List<Bot>();
                context.Entry(user).Collection(u => u.CreatedBots).Load();
                var creations = user.CreatedBots.ToList();
                foreach (var botCreation in user.CreatedBots)
                {
                    context.Entry(botCreation).Reference(bc => bc.Bot).Load();
                    context.Entry(botCreation.Bot).Reference(b => b.BotSecret).Load();
                    bots.Add(botCreation.Bot);
                }
                return new Packet {Status = "success", Bots = bots, BotCreations = creations};
            }
        }

        [Route("~/api/robot/get_subscribed_bots")]
        [HttpPost]
        public ActionResult<Packet> GetSubscribedBots()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                var bots = new List<Bot>();
                context.Entry(user).Collection(u => u.SubscribedBots).Load();
                var subscriptions = user.SubscribedBots.ToList();
                var noAccessBotIds = new List<long>();
                foreach (var botSubscription in user.SubscribedBots)
                {
                    context.Entry(botSubscription).Reference(bc => bc.Bot).Load();
                    context.Entry(botSubscription.Bot).Reference(b => b.BotSecret).Load();
                    if (botSubscription.Bot.BotSecret.CreatorId == user.BaseUserId)
                    {
                        bots.Add(botSubscription.Bot);
                    }
                    else
                    {
                        noAccessBotIds.Add(botSubscription.Bot.BaseUserId);
                    }
                }
                using (var nextContext = new DatabaseContext())
                {
                    foreach (var id in noAccessBotIds)
                    {
                        var bot = nextContext.Bots.Find(id);
                        bots.Add(bot);
                    }
                }
                return new Packet {Status = "success", Bots = bots, BotSubscriptions = subscriptions};
            }
        }

        [Route("~/api/robot/subscribe_bot")]
        [HttpPost]
        public ActionResult<Packet> SubscribeBot([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_2"};
                var bot = context.Bots.Find(packet.Bot.BaseUserId);
                if (bot == null) return new Packet {Status = "error_1"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.SubscribedBots).Load();
                if (user.SubscribedBots.Any(b => b.BotId == packet.Bot.BaseUserId))
                    return new Packet {Status = "error_0"};
                var subscription = new BotSubscription()
                {
                    Bot = bot,
                    Subscriber = user
                };
                context.AddRange(subscription);
                context.SaveChanges();
                return new Packet {Status = "success", BotSubscription = subscription};
            }
        }

        [Route("~/api/robot/create_bot")]
        [HttpPost]
        public ActionResult<Packet> CreateBot([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                var bot = new Bot()
                {
                    Title = packet.Bot.Title,
                    Avatar = packet.Bot.Avatar > 0 ? packet.Bot.Avatar : 0,
                    ViewURL = packet.Bot.ViewURL
                };
                var botSecret = new BotSecret()
                {
                    Bot = bot,
                    Creator = user,
                    Token = Security.MakeKey64()
                };
                bot.BotSecret = botSecret;
                var botSess = new Session()
                {
                    Token = botSecret.Token,
                    BaseUser = bot
                };
                var botCreation = new BotCreation()
                {
                    Bot = bot,
                    Creator = user
                };
                var subscription = new BotSubscription()
                {
                    Bot = bot,
                    Subscriber = user
                };
                context.AddRange(bot, botSecret, botSess, botCreation, subscription);
                context.SaveChanges();
                return new Packet
                {
                    Status = "success",
                    Bot = bot,
                    BotCreation = botCreation,
                    BotSubscription = subscription
                };
            }
        }

        [Route("~/api/robot/get_robot")]
        [HttpPost]
        public ActionResult<Packet> GetRobot([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_081"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var robot = context.Bots.Find(packet.Bot.BaseUserId);
                if (robot == null) return new Packet {Status = "error_080"};
                context.Entry(robot).Reference(r => r.BotSecret).Load();
                if (robot.BotSecret.CreatorId == session.BaseUserId)
                    return new Packet {Status = "success", Bot = robot};
                using (var nextContext = new DatabaseContext())
                {
                    var nextBot = nextContext.Bots.Find(packet.Bot.BaseUserId);
                    return new Packet {Status = "success", Bot = nextBot};
                }
            }
        }

        [Route("~/api/robot/update_bot_profile")]
        [HttpPost]
        public ActionResult<Packet> UpdateBotProfile([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.CreatedBots).Load();
                var botCreation = user.CreatedBots.Find(bc => bc.BotId == packet.Bot.BaseUserId);
                if (botCreation == null) return new Packet {Status = "error_0"};
                context.Entry(botCreation).Reference(bc => bc.Bot).Load();
                var bot = botCreation.Bot;
                bot.Title = packet.Bot.Title;
                bot.Avatar = packet.Bot.Avatar;
                bot.ViewURL = packet.Bot.ViewURL;
                context.SaveChanges();
                return new Packet {Status = "success"};
            }
        }

        [Route("~/api/robot/get_workerships")]
        [HttpPost]
        public ActionResult<Packet> GetWorkerships([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_1"};
                context.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_2"};
                context.Entry(room).Collection(r => r.Workers).Load();
                var workers = room.Workers.ToList();
                return new Packet {Status = "success", Workerships = workers};
            }
        }

        [Route("~/api/robot/search_bots")]
        [HttpPost]
        public ActionResult<Packet> SearchBots([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var bots = (from b in context.Bots
                    where EF.Functions.Like(b.Title, "%" + packet.SearchQuery + "%")
                    select b).ToList();
                return new Packet {Status = "success", Bots = bots};
            }
        }

        [Route("~/api/robot/remove_bot_from_room")]
        [HttpPost]
        public ActionResult<Packet> RemoveBotFromRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_2"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_1"};
                context.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_3"};
                context.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == packet.Bot.BaseUserId);
                if (workership == null) return new Packet {Status = "error_0"};
                room.Workers.Remove(workership);
                context.Workerships.Remove(workership);
                context.SaveChanges();
                var bot = context.Bots.Find(workership.BotId);
                context.Entry(bot).Collection(b => b.Sessions).Load();
                var botSess = bot.Sessions.FirstOrDefault();
                var removation = new BotRemovationFromRoomNotification()
                {
                    Room = room,
                    Session = botSess
                };
                if (botSess.Online)
                {
                    _notifsHub.Clients.Client(botSess.ConnectionId)
                        .SendAsync("NotifyBotRemovedFromRoom", removation);
                }
                else
                {
                    context.Notifications.Add(removation);
                }
                context.SaveChanges();
                return new Packet {Status = "success"};
            }
        }
    }
}