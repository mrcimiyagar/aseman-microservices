using System;
using System.Collections.Generic;
using System.Linq;
using MessengerPlatform.DbContexts;
using AWP.Hubs;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using AWP.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class InviteController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public InviteController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
        [Route("~/api/invite/create_invite")]
        [HttpPost]
        public ActionResult<Packet> CreateInvite([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session ==  null) return new Packet {Status = "error_0H3"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var human = context.Users.Find(packet.User.BaseUserId);
                context.Entry(human).Collection(h => h.Memberships).Load();
                if (human.Memberships.All(m => m.ComplexId != packet.Complex.ComplexId))
                {
                    context.Entry(human).Collection(h => h.Invites).Load();
                    if (human.Invites.All(i => i.ComplexId != packet.Complex.ComplexId))
                    {
                        var complex = context.Complexes.Find(packet.Complex.ComplexId);
                        if (complex != null)
                        {
                            context.Entry(complex).Reference(c => c.ComplexSecret).Load();
                            context.Entry(session).Reference(s => s.BaseUser).Load();
                            if (complex.ComplexSecret.AdminId == session.BaseUser.BaseUserId)
                            {
                                var invite = new Invite()
                                {
                                    Complex = complex,
                                    User = human
                                };
                                context.AddRange(invite);
                                context.SaveChanges();
                                context.Entry(human).Collection(h => h.Sessions).Load();
                                foreach (var targetSession in human.Sessions)
                                {
                                    var inviteNotification = new InviteCreationNotification()
                                    {
                                        Invite = invite,
                                        Session = targetSession
                                    };
                                    if (targetSession.Online)
                                    {
                                        _notifsHub.Clients.Client(targetSession.ConnectionId)
                                            .SendAsync("NotifyInviteCreated", inviteNotification);
                                    }
                                    else
                                    {
                                        context.Notifications.Add(inviteNotification);
                                    }
                                }
                                context.SaveChanges();
                                return new Packet {Status = "success", Invite = invite};
                            }
                            else
                                return new Packet {Status = "error_0H0"};
                        }
                        else
                            return new Packet {Status = "error_0H4"};
                    }
                    else
                        return new Packet {Status = "error_0H1"};
                }
                else
                    return new Packet {Status = "error_0H2"};
            }
        }

        [Route("~/api/invite/cancel_invite")]
        [HttpPost]
        public ActionResult<Packet> CancelInvite([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0I1"};
                var complex = context.Complexes.Find(packet.Complex.ComplexId);
                if (complex != null)
                {
                    context.Entry(complex).Collection(c => c.Invites).Load();
                    var invite = complex.Invites.Find(i => i.UserId == packet.User.BaseUserId);
                    if (invite != null)
                    {
                        context.Entry(invite).Reference(i => i.User).Load();
                        var human = invite.User;
                        context.Entry(human).Collection(h => h.Invites).Load();
                        human.Invites.Remove(invite);
                        context.Invites.Remove(invite);
                        context.SaveChanges();
                        context.Entry(human).Collection(h => h.Sessions).Load();
                        foreach (var targetSession in human.Sessions)
                        {
                            var notification = new InviteCancellationNotification
                            {
                                Invite = invite,
                                Session = targetSession
                            };
                            if (targetSession.Online)
                            {
                                _notifsHub.Clients.Client(targetSession.ConnectionId)
                                    .SendAsync("NotifyInviteCancelled", notification);
                            }
                            else
                            {
                                context.Notifications.Add(notification);
                            }
                        }
                        context.SaveChanges();
                        return new Packet {Status = "success"};
                    }
                    else
                    {
                        return new Packet {Status = "error_0I0"};
                    }
                } 
                else
                {
                    return new Packet {Status = "error_0I2"};
                }
            }
        }

        [Route("~/api/invite/accept_invite")]
        [HttpPost]
        public ActionResult<Packet> AcceptInvite([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0J1"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                context.Entry(human).Collection(h => h.Invites).Load();
                var invite = human.Invites.Find(i => i.ComplexId == packet.Complex.ComplexId);
                if (invite != null)
                {
                    context.Entry(invite).Reference(i => i.Complex).Load();
                    var complex = invite.Complex;
                    human.Invites.Remove(invite);
                    context.Invites.Remove(invite);
                    var membership = new Membership
                    {
                        User = human,
                        Complex = complex
                    };
                    context.Entry(complex).Collection(c => c.Members).Load();
                    complex.Members.Add(membership);
                    context.Entry(complex).Collection(c => c.Rooms).Load();
                    var hall = complex.Rooms.FirstOrDefault() ?? new Room();
                    var message = new ServiceMessage()
                    {
                        Room = hall,
                        Author = null,
                        Text = human.Title + " entered complex by invite.",
                        Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds)
                    };
                    context.Entry(hall).Collection(r => r.Messages).Load();
                    hall.Messages.Add(message);
                    context.SaveChanges();
                    User user;
                    Complex jointComplex;
                    using (var context2 = new DatabaseContext())
                    {
                        user = context2.Users.Find(human.BaseUserId);
                        jointComplex = context2.Complexes.Find(complex.ComplexId);
                    }
                    foreach (var m in complex.Members) 
                    {
                        if (m.UserId == session.BaseUserId) continue;
                        context.Entry(m).Reference(mem => mem.User).Load();
                        var member = m.User;
                        context.Entry(member).Collection(u => u.Sessions).Load();
                        foreach (var s in member.Sessions)
                        {
                            var mcn = new ServiceMessageNotification
                            {
                                Message = message,
                                Session = s
                            };
                            var ujn = new UserJointComplexNotification
                            {
                                UserId = user.BaseUserId,
                                ComplexId = jointComplex.ComplexId,
                                Session = s
                            };                            
                            if (s.Online)
                            {
                                _notifsHub.Clients.Client(s.ConnectionId)
                                    .SendAsync("NotifyUserJointComplex", ujn);
                                _notifsHub.Clients.Client(s.ConnectionId)
                                    .SendAsync("NotifyServiceMessageReceived", mcn);
                            } 
                            else
                            {
                                context.Notifications.Add(ujn);
                                context.Notifications.Add(mcn);
                            }
                        }
                    }
                    context.SaveChanges();
                    context.Entry(complex).Reference(c => c.ComplexSecret).Load();
                    context.Entry(complex.ComplexSecret).Reference(cs => cs.Admin).Load();
                    var inviter = complex.ComplexSecret.Admin;
                    context.Entry(inviter).Collection(i => i.Sessions).Load();
                    foreach (var targetSession in inviter.Sessions)
                    {
                        var notification = new InviteAcceptanceNotification
                        {
                            Invite = invite,
                            Session = targetSession
                        };
                        if (targetSession.Online)
                        {
                            _notifsHub.Clients.Client(targetSession.ConnectionId)
                                .SendAsync("NotifyInviteAccepted", notification);
                        }
                        else
                        {
                            context.Notifications.Add(notification);
                        }
                    }
                    context.SaveChanges();
                    return new Packet { Status = "success" };
                }
                else
                {
                    return new Packet { Status = "error_0J0" };
                }
            }
        }

        [Route("~/api/invite/ignore_invite")]
        [HttpPost]
        public ActionResult<Packet> IgnoreInvite([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0K1"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                context.Entry(human).Collection(h => h.Invites).Load();
                var invite = human.Invites.Find(i => i.ComplexId == packet.Complex.ComplexId);
                if (invite != null)
                {
                    context.Entry(invite).Reference(i => i.Complex).Load();
                    var complex = invite.Complex;
                    human.Invites.Remove(invite);
                    context.Invites.Remove(invite);
                    context.Entry(complex).Reference(c => c.ComplexSecret).Load();
                    context.Entry(complex.ComplexSecret).Reference(cs => cs.Admin).Load();
                    var inviter = complex.ComplexSecret.Admin;
                    context.Entry(inviter).Collection(i => i.Sessions).Load();
                    foreach (var targetSession in inviter.Sessions) 
                    {
                        var notification = new InviteIgnoranceNotification
                        {
                            Invite = invite,
                            Session = targetSession
                        };
                        if (targetSession.Online)
                        {
                            _notifsHub.Clients.Client(targetSession.ConnectionId)
                                .SendAsync("NotifyInviteIgnored", notification);
                        }
                        else
                        {
                            context.Notifications.Add(notification);
                        }
                    }
                    context.SaveChanges();
                    return new Packet {Status = "success"};
                } 
                else
                {
                    return new Packet {Status = "error_0K0"};
                }
            }
        }
    }
}