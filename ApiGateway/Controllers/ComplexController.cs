
using System.Collections.Generic;
using System.Linq;
using MessengerPlatform.DbContexts;
using AWP.Hubs;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using AWP.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Clauses;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ComplexController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public ComplexController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }

        [Route("~/api/complex/update_complex_profile")]
        [HttpPost]
        public ActionResult<Packet> UpdateProfile([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_1" };
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var complex = context.Complexes.Include(c => c.ComplexSecret)
                    .SingleOrDefault(c => c.ComplexId == packet.Complex.ComplexId);
                if (complex?.ComplexSecret.AdminId == session.BaseUser.BaseUserId)
                {
                    complex.Title = packet.Complex.Title;
                    complex.Avatar = packet.Complex.Avatar;
                    context.SaveChanges();
                    return new Packet { Status = "success", Complex = complex};
                }
                else
                    return new Packet { Status = "error_0" };
            }
        }

        [Route("~/api/complex/create_complex")]
        [HttpPost]
        public ActionResult<Packet> CreateComplex([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_1" };
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                if (!string.IsNullOrEmpty(packet.Complex.Title) && packet.Complex.Title.ToLower() != "home")
                {
                    context.Entry(user).Reference(u => u.UserSecret).Load();
                    var complex = new Complex
                    {
                        Title = packet.Complex.Title,
                        Avatar = packet.Complex.Avatar > 0 ? packet.Complex.Avatar : 0,
                        Mode = 3
                    };
                    var cs = new ComplexSecret()
                    {
                        Admin = user,
                        Complex = complex,
                    };
                    complex.ComplexSecret = cs;
                    var room = new Room()
                    {
                        Title = "Hall",
                        Avatar = 0,
                        Complex = complex
                    };
                    var mem = new Membership()
                    {
                        Complex = complex,
                        User = user
                    };
                    context.AddRange(complex, cs, room, mem);
                    context.SaveChanges();
                        
                    context.Entry(complex).Collection(c => c.Rooms).Load();

                    return new Packet { Status = "success", Complex = complex };
                }
                else
                {
                    return new Packet { Status = "error_0" };
                }
            }
        }

        [Route("~/api/complex/delete_complex")]
        [HttpPost]
        public ActionResult<Packet> DeleteComplex([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };
                var complex = context.Complexes.Find(packet.Complex.ComplexId);
                if (complex != null) 
                {
                    if (complex.Title != "" && complex.Title.ToLower() != "home")
                    {
                        context.Entry(complex).Collection(c => c.Members).Load();
                        var members = complex.Members.ToList();
                        foreach (var membership in members)
                        {
                            context.Entry(membership).Reference(m => m.User).Load();
                            var user = membership.User;
                            context.Entry(user).Collection(u => u.Memberships).Load();
                            user.Memberships.Remove(membership);
                            foreach (var memSess in user.Sessions)
                            {
                                var cdn = new ComplexDeletionNotification()
                                {
                                    ComplexId = complex.ComplexId,
                                    Session = memSess
                                };
                                if (memSess.Online)
                                    _notifsHub.Clients.Client(memSess.ConnectionId)
                                        .SendAsync("NotifyComplexDeleted", cdn);
                                else
                                {
                                    context.Notifications.Add(cdn);
                                }
                            }

                            context.Memberships.Remove(membership);
                        }
                        context.Entry(complex).Collection(c => c.Rooms).Load();
                        foreach (var room in complex.Rooms)
                        {
                            context.Rooms.Remove(room);
                        }
                        context.Entry(complex).Reference(c => c.ComplexSecret).Load();
                        context.ComplexSecrets.Remove(complex.ComplexSecret);
                        context.Complexes.Remove(complex);

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

        [Route("~/api/complex/get_complexes")]
        [HttpPost]
        public ActionResult<Packet> GetComplexes()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var memberships = user.Memberships;
                var complexes = new List<Complex>();
                foreach (var membership in memberships)
                {
                    context.Entry(membership).Reference(m => m.Complex).Load();
                    complexes.Add(membership.Complex);
                }
                return new Packet {Status = "success", Complexes = complexes};
            }
        }

        [Route("~/api/complex/get_complex_by_id")]
        [HttpPost]
        public ActionResult<Packet> GetComplexById([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                var complex = context.Complexes.Find(packet.Complex.ComplexId);
                if (complex == null) return new Packet {Status = "error_1"};
                if (complex.Mode == 3)
                    return new Packet {Status = "success", Complex = context.Complexes.Find(packet.Complex.ComplexId)};
                else if (complex.Mode == 2)
                {
                    context.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
                    context.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                    if (membership == null)
                        return new Packet {Status = "error_2"};
                    else
                    {
                        context.Entry(membership).Reference(m => m.Complex).Load();
                        return new Packet {Status = "success", Complex = membership.Complex};
                    }
                }
                else
                {
                    context.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
                    context.Entry(user).Reference(u => u.UserSecret).Load();
                    if (user.UserSecret.HomeId == packet.Complex.ComplexId)
                    {
                        context.Entry(user.UserSecret).Reference(us => us.Home).Load();
                        return new Packet { Status = "success", Complex = user.UserSecret.Home };
                    }
                    else
                    {
                        return new Packet {Status = "error_3"};
                    }
                }
            }
        }

        [Route("~/api/complex/search_complexes")]
        [HttpPost]
        public ActionResult<Packet> SearchComplexes([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Query().Include(m => m.Complex).Load();
                var complexes = (from c in (from m in user.Memberships
                    where EF.Functions.Like(m.Complex.Title, "%" + packet.SearchQuery + "%")
                    select m) select c.Complex).ToList();
                return new Packet {Status = "success", Complexes = complexes};
            }
        }
    }
}