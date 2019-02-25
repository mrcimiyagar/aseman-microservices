using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using SharedArea.Commands.Auth;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.DbContexts;
using SharedArea.Entities;

namespace SharedArea.Utils
{
    public class GlobalConsumer<T> : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>, IConsumer<RoomCreatedNotif>
        , IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>, IConsumer<UserProfileUpdatedNotif>
        , IConsumer<ComplexProfileUpdatedNotif>, IConsumer<ComplexDeletionNotif>, IConsumer<RoomProfileUpdatedNotif>
        , IConsumer<RoomDeletionNotif>, IConsumer<ContactCreatedNotif>, IConsumer<InviteCreatedNotif>
        , IConsumer<InviteCancelledNotif>, IConsumer<InviteAcceptedNotif>, IConsumer<InviteIgnoredNotif>
        , IConsumer<SessionUpdatedNotif>, IConsumer<ConsolidateLogoutRequest>, IConsumer<AccountCreatedNotif>
        , IConsumer<DeleteAccountRequest>

        where T : DatabaseContext
    
    {
        public async Task Consume(ConsumeContext<DeleteAccountRequest> context)
        {
            var gUser = context.Message.Packet.User;

            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var user = (User) dbContext.BaseUsers.Find(gUser.BaseUserId);

                if (user != null)
                {
                    dbContext.Entry(user).Collection(u => u.Sessions).Load();
                    dbContext.Entry(user).Reference(u => u.UserSecret).Load();

                    user.Title = "Deleted User";
                    user.Avatar = -1;
                    user.UserSecret.Email = "";
                    dbContext.Sessions.RemoveRange(user.Sessions);

                    dbContext.SaveChanges();
                }
            }

            await context.RespondAsync(new DeleteAccountResponse());
        }
        
        public async Task Consume(ConsumeContext<UserCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var user = context.Message.Packet.User;
                var userSecret = context.Message.Packet.UserSecret;

                user.UserSecret = userSecret;
                userSecret.User = user;

                dbContext.AddRange(user, userSecret);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new UserCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ComplexCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var admin = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);
                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;

                complex.ComplexSecret = complexSecret;
                complexSecret.Complex = complex;
                complex.Rooms[0].Complex = complex;
                complex.Members[0].Complex = complex;
                complexSecret.Admin = admin;
                complex.Members[0].User = admin;

                dbContext.AddRange(complex);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ComplexCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<RoomCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var room = context.Message.Packet.Room;
                var complex = dbContext.Complexes.Find(room.ComplexId);
                
                room.Complex = complex;

                dbContext.AddRange(room);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new RoomCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<MembershipCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var membership = context.Message.Packet.Membership;
                var user = context.Message.Packet.User;
                var complex = context.Message.Packet.Complex;

                membership.User = user;
                membership.Complex = complex;

                dbContext.AddRange(membership);

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new MembershipCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<SessionCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var session = context.Message.Packet.Session;
                var user = context.Message.Packet.BaseUser;

                session.BaseUser = dbContext.BaseUsers.Find(user.BaseUserId);

                dbContext.AddRange(session);

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new SessionCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<UserProfileUpdatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var globalUser = context.Message.Packet.BaseUser;

                var localUser = dbContext.BaseUsers.Find(globalUser.BaseUserId);

                localUser.Title = globalUser.Title;
                localUser.Avatar = globalUser.Avatar;

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new UserProfileUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ComplexProfileUpdatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var globalComplex = context.Message.Packet.Complex;

                var localComplex = dbContext.Complexes.Find(globalComplex.ComplexId);

                localComplex.Title = globalComplex.Title;
                localComplex.Avatar = globalComplex.Avatar;

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ComplexProfileUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ComplexDeletionNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var complex = dbContext.Complexes.Find(context.Message.Packet.Complex.ComplexId);

                dbContext.Entry(complex).Collection(c => c.Members).Load();
                var members = complex.Members.ToList();
                foreach (var membership in members)
                {
                    dbContext.Entry(membership).Reference(m => m.User).Load();
                    var user = membership.User;
                    dbContext.Entry(user).Collection(u => u.Memberships).Load();
                    user.Memberships.Remove(membership);
                    dbContext.Memberships.Remove(membership);

                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    foreach (var room in complex.Rooms)
                    {
                        dbContext.Rooms.Remove(room);
                    }

                    dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                    dbContext.ComplexSecrets.Remove(complex.ComplexSecret);
                    dbContext.Complexes.Remove(complex);

                    dbContext.SaveChanges();
                }
            }

            await context.RespondAsync(new ComplexDeletionNotifResponse());
        }

        public async Task Consume(ConsumeContext<RoomProfileUpdatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var globalRoom = context.Message.Packet.Room;

                var localRoom = dbContext.Rooms.Find(globalRoom.RoomId);

                localRoom.Title = globalRoom.Title;
                localRoom.Avatar = globalRoom.Avatar;

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ComplexDeletionNotifResponse());
        }

        public async Task Consume(ConsumeContext<RoomDeletionNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var globalRoom = context.Message.Packet.Room;

                var localRoom = dbContext.Rooms.Find(globalRoom.RoomId);

                dbContext.Entry(localRoom).Reference(r => r.Complex).Load();
                var complex = localRoom.Complex;
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();

                complex.Rooms.Remove(localRoom);
                dbContext.Rooms.Remove(localRoom);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new RoomDeletionNotifResponse());
        }

        public async Task Consume(ConsumeContext<ContactCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var me = (User) dbContext.BaseUsers.Find(context.Message.Packet.Users[0].BaseUserId);
                var peer = (User) dbContext.BaseUsers.Find(context.Message.Packet.Users[1].BaseUserId);

                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;
                var room = context.Message.Packet.Room;
                var m1 = context.Message.Packet.Memberships[0];
                var m2 = context.Message.Packet.Memberships[1];

                var lComplex = new Complex()
                {
                    ComplexId = complex.ComplexId,
                    Title = complex.Title,
                    Avatar = complex.Avatar,
                    Mode = complex.Mode,
                    ComplexSecret = new ComplexSecret()
                    {
                        ComplexSecretId = complexSecret.ComplexSecretId,
                        Admin = null
                    },
                    Rooms = new List<Room>()
                    {
                        new Room()
                        {
                            RoomId = room.RoomId,
                            Title = room.Title,
                            Avatar = room.Avatar
                        }
                    },
                    Members = new List<Membership>()
                    {
                        new Membership()
                        {
                            MembershipId = m1.MembershipId,
                            User = me
                        },
                        new Membership()
                        {
                            MembershipId = m2.MembershipId,
                            User = peer
                        }
                    }
                };

                dbContext.AddRange(lComplex);
                dbContext.SaveChanges();

                var myContact = context.Message.Packet.Contacts[0];
                myContact.Complex = lComplex;
                myContact.User = me;
                myContact.Peer = peer;
                dbContext.Contacts.Add(myContact);

                var peerContact = context.Message.Packet.Contacts[1];
                peerContact.Complex = lComplex;
                peerContact.User = peer;
                peerContact.Peer = me;
                dbContext.Contacts.Add(peerContact);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ContactCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var invite = context.Message.Packet.Invite;
                var complex = dbContext.Complexes.Find(context.Message.Packet.Complex.ComplexId);
                var user = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);

                invite.Complex = complex;
                invite.User = user;

                dbContext.AddRange(invite);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new InviteCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteCancelledNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var invite = dbContext.Invites.Find(context.Message.Packet.Invite.InviteId);
                var user = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);

                user.Invites.Remove(invite);
                dbContext.Invites.Remove(invite);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new InviteCancelledNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteAcceptedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var invite = dbContext.Invites.Find(context.Message.Packet.Invite.InviteId);
                var membership = context.Message.Packet.Membership;
                var message = context.Message.Packet.ServiceMessage;
                var human = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);

                dbContext.Entry(invite).Reference(i => i.Complex).Load();
                var complex = invite.Complex;
                human.Invites.Remove(invite);
                dbContext.Invites.Remove(invite);
                membership.Complex = complex;
                membership.User = human;
                dbContext.Entry(complex).Collection(c => c.Members).Load();
                complex.Members.Add(membership);
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var hall = complex.Rooms.FirstOrDefault();
                message.Room = hall;
                dbContext.Messages.Add(message);
                dbContext.SaveChanges();
            }

            await context.RespondAsync(new InviteAcceptedNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteIgnoredNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var invite = dbContext.Invites.Find(context.Message.Packet.Invite.InviteId);
                var human = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);

                dbContext.Entry(invite).Reference(i => i.Complex).Load();
                human.Invites.Remove(invite);
                dbContext.Invites.Remove(invite);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new InviteIgnoredNotifResponse());
        }

        public async Task Consume(ConsumeContext<SessionUpdatedNotif> context)
        {
            var globalSession = context.Message.Packet.Session;

            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var session = dbContext.Sessions.Find(globalSession.SessionId);

                session.Online = globalSession.Online;
                session.ConnectionId = globalSession.ConnectionId;
                session.Token = globalSession.Token;

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new SessionUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<AccountCreatedNotif> context)
        {
            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var user = context.Message.Packet.User;
                var userSecret = context.Message.Packet.UserSecret;
                var complexSecret = context.Message.Packet.ComplexSecret;

                user.UserSecret = userSecret;
                userSecret.User = user;
                user.Memberships[0].User = user;
                user.Memberships[0].Complex.ComplexSecret = complexSecret;
                user.Memberships[0].Complex.ComplexSecret.Complex = user.Memberships[0].Complex;
                user.Memberships[0].Complex.ComplexSecret.Admin = user;
                user.Memberships[0].Complex.Rooms[0].Complex = user.Memberships[0].Complex;
                user.UserSecret.Home = user.Memberships[0].Complex;
                user.Memberships[0].User = user;
                
                dbContext.AddRange(user);

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new AccountCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ConsolidateLogoutRequest> context)
        {
            var gSession = context.Message.Packet.Session;

            using (var dbContext = (DatabaseContext) Activator.CreateInstance<T>())
            {
                var lSess = dbContext.Sessions.Find(gSession.SessionId);
                if (lSess != null)
                {
                    dbContext.Sessions.RemoveRange(lSess);
                    dbContext.SaveChanges();
                }
            }

            await context.RespondAsync(new ConsolidateLogoutResponse());
        }
    }
}