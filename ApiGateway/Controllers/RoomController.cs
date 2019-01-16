using System;
using System.Linq;
using MessengerPlatform.DbContexts;
using AWP.Hubs;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using AWP.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.SignalR;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public RoomController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
        [Route("~/api/room/update_room_profile")]
        [HttpPost]
        public ActionResult<Packet> UpdateProfile([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };
                var complex = context.Complexes.Find(packet.Room.ComplexId);
                context.Entry(complex).Reference(c => c.ComplexSecret).Load();
                context.Entry(session).Reference(s => s.BaseUser).Load();
                if (complex.ComplexSecret.AdminId == session.BaseUser.BaseUserId)
                {
                    context.Entry(complex).Collection(c => c.Rooms).Load();
                    if (complex.Rooms.Any(r => r.RoomId == packet.Room.RoomId))
                    {
                        var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                        room.Title = packet.Room.Title;
                        room.Avatar = packet.Room.Avatar;
                        context.SaveChanges();
                        return new Packet { Status = "success" };
                    }
                    else
                        return new Packet { Status = "error_0" };
                }
                else
                    return new Packet { Status = "error_1" };
            }
        }

        [Route("~/api/room/create_room")]
        [HttpPost]
        public ActionResult<Packet> CreateRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };
                var complex = context.Complexes.Find(packet.Complex.ComplexId);
                if (complex != null)
                {
                    if (!string.IsNullOrEmpty(packet.Room.Title))
                    {
                        var room = new Room()
                        {
                            Title = packet.Room.Title,
                            Avatar = 0,
                            Complex = complex
                        };
                        context.AddRange(room);
                        context.SaveChanges();
                        return new Packet { Status = "success", Room = room };
                    }
                    else
                        return new Packet { Status = "error_0" };
                }
                else
                    return new Packet { Status = "error_1" };
            }
        }

        [Route("~/api/room/delete_room")]
        [HttpPost]
        public ActionResult<Packet> DeleteRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                context.Entry(human).Collection(u => u.Memberships).Load();
                if (human.Memberships.Any(m => m.ComplexId == packet.Room.ComplexId)) 
                {
                    var complex = context.Complexes.Find(packet.Room.ComplexId);
                    context.Entry(complex).Collection(c => c.Rooms).Load();
                    if (complex.Rooms.Any(r => r.RoomId == packet.Room.RoomId))
                    {
                        var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                        complex.Rooms.Remove(room);
                        context.Rooms.Remove(room);
                        context.SaveChanges();
                        context.Entry(complex).Collection(c => c.Members).Load();
                        foreach (var ms in complex.Members)
                        {
                            try
                            {
                                context.Entry(ms).Reference(mem => mem.User).Load();
                                context.Entry(ms.User).Collection(u => u.Sessions).Load();
                                foreach (var userSession in ms.User.Sessions)
                                {
                                    var rdn = new RoomDeletionNotification()
                                    {
                                        ComplexId = complex.ComplexId,
                                        RoomId = room.RoomId,
                                        Session = userSession
                                    };
                                    if (userSession.Online)
                                        _notifsHub.Clients.Client(userSession.ConnectionId)
                                            .SendAsync("NotifyRoomDeleted", rdn);
                                    else
                                    {
                                        context.Notifications.Add(rdn);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        context.SaveChanges();
                        return new Packet { Status = "success" };
                    }
                    else
                        return new Packet { Status = "error_0" };
                }
                else
                    return new Packet { Status = "error_1" };
            }
        }

        [Route("~/api/room/get_rooms")]
        [HttpPost]
        public ActionResult<Packet> GetRooms([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0B1"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(mem => mem.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_0B0"};
                context.Entry(membership).Reference(mem => mem.Complex).Load();
                context.Entry(membership.Complex).Collection(c => c.Rooms).Load();
                return new Packet {Status = "success", Rooms = membership.Complex.Rooms};
            }
        }

        [Route("~/api/room/get_room_by_id")]
        [HttpPost]
        public ActionResult<Packet> GetRoomById([FromBody] Packet packet)
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
                context.Entry(membership.Complex).Collection(c => c.Rooms).Load();
                var room = membership.Complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                return room == null ? new Packet {Status = "error_2"} : new Packet {Status = "success", Room = room};
            }
        }
    }
}