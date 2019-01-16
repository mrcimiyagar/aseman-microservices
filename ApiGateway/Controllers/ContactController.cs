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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public ContactController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
        [Route("~/api/contact/create_contact")]
        [HttpPost]
        public ActionResult<Packet> GetContacts([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_051"};

                context.Entry(session).Reference(s => s.BaseUser).Load();
                var me = (User) session.BaseUser;
                var peer = context.Users.Find(packet.User.BaseUserId);

                var contactExists = false;

                context.Entry(me).Collection(u => u.Contacts).Load();
                foreach (var contact in me.Contacts) 
                {
                    if (contact.PeerId != packet.User.BaseUserId) continue;
                    contactExists = true;
                    break;
                }

                if (!contactExists)
                {
                    var complex = new Complex
                    {
                        Title = "",
                        Avatar = -1
                    };
                    var complexSecret = new ComplexSecret
                    {
                        Admin = null,
                        Complex = complex
                    };
                    complex.ComplexSecret = complexSecret;
                    var room = new Room()
                    {
                        Title = "Hall",
                        Avatar = 0,
                        Complex = complex
                    };
                    var m1 = new Membership()
                    {
                        User = me,
                        Complex = complex
                    };
                    var m2 = new Membership()
                    {
                        User = peer,
                        Complex = complex
                    };
                    var message = new ServiceMessage
                    {
                        Text = "Room created.",
                        Room = room,
                        Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds),
                        Author = null
                    };
                    var myContact = new Contact
                    {
                        Complex = complex,
                        User = me,
                        Peer = peer
                    };
                    var peerContact = new Contact
                    {
                        Complex = complex,
                        User = peer,
                        Peer = me
                    };
                    context.AddRange(complex, complexSecret, room, m1, m2, message, myContact, peerContact);
                    context.SaveChanges();
                    
                    context.Entry(peerContact.Complex).Collection(c => c.Rooms).Load();
                    
                    context.Entry(peer).Collection(u => u.Sessions).Load();
                    foreach(var s in peer.Sessions) 
                    {
                        var ccn = new ContactCreationNotification
                        {
                            Contact = peerContact,
                            Session = s
                        };
                        var mcn = new ServiceMessageNotification
                        {
                            Message = message,
                            Session = s
                        };
                        if (s.Online)
                        {
                            _notifsHub.Clients.Client(s.ConnectionId)
                                .SendAsync("NotifyContactCreated", ccn);
                            _notifsHub.Clients.Client(s.ConnectionId)
                                .SendAsync("NotifyServiceMessageReceived", mcn);
                        } 
                        else
                        {
                            context.Notifications.Add(ccn);
                            context.Notifications.Add(mcn);
                        }
                    }

                    context.SaveChanges();
                    
                    ServiceMessage finalMessage;
                    Contact finalMyContact;
                    using (var finalContext = new DatabaseContext())
                    {
                        finalMessage = (ServiceMessage) finalContext.Messages.Find(message.MessageId);
                        finalMyContact = finalContext.Contacts.Find(myContact.ContactId);
                        finalContext.Entry(finalMessage).Reference(m => m.Room).Load();
                        finalContext.Entry(finalMyContact).Reference(c => c.Complex).Load();
                        finalContext.Entry(finalMyContact.Complex).Collection(c => c.Rooms).Load();
                        finalContext.Entry(finalMyContact).Reference(c => c.Peer).Load();
                    }

                    return new Packet {Status = "success", Contact = finalMyContact, ServiceMessage = finalMessage};
                } 
                else
                {
                    return new Packet { Status = "error_050" };
                }
            }
        }

        [Route("~/api/contact/get_contacts")]
        [HttpPost]
        public ActionResult<Packet> GetContacts()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_060"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Contacts).Load();
                var contacts = user.Contacts;
                foreach (var contact in contacts)
                {
                    context.Entry(contact).Reference(c => c.Complex).Load();
                    context.Entry(contact).Reference(c => c.Peer).Load();
                    context.Entry(contact).Reference(c => c.User).Load();
                }
                return new Packet {Status = "success", Contacts = contacts};
            }
        }
    }
}