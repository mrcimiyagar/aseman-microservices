
using System.Linq;
using System.Threading.Tasks;
using BotPlatform.DbContexts;
using MassTransit;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Entities;

namespace BotPlatform.Consumers
{
    public class NotifConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>, IConsumer<RoomCreatedNotif>
        , IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>, IConsumer<UserProfileUpdatedNotif>
        , IConsumer<ComplexProfileUpdatedNotif>, IConsumer<ComplexDeletionNotif>, IConsumer<RoomProfileUpdatedNotif>
        , IConsumer<RoomDeletionNotif>, IConsumer<ContactCreatedNotif>, IConsumer<InviteCreatedNotif>
        , IConsumer<InviteCancelledNotif>, IConsumer<InviteAcceptedNotif>, IConsumer<InvitedIgnoredNotif>
        , IConsumer<SessionUpdatedNotif>
    {
        public Task Consume(ConsumeContext<UserCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var user = context.Message.Packet.User;
                var userSecret = context.Message.Packet.UserSecret;

                user.UserSecret = userSecret;
                userSecret.User = user;
                
                dbContext.AddRange(user, userSecret);

                dbContext.SaveChanges();
                
                return Task.CompletedTask;
            }
        }

        public Task Consume(ConsumeContext<ComplexCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var admin = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);
                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;

                complex.ComplexSecret = complexSecret;
                complexSecret.Admin = admin;
                complexSecret.Complex = complex;

                dbContext.AddRange(complex, complexSecret);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoomCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var complex = context.Message.Packet.Complex;
                var room = context.Message.Packet.Room;
                
                room.Complex = complex;

                dbContext.AddRange(room);

                dbContext.SaveChanges();

                return Task.CompletedTask;
            }
        }

        public Task Consume(ConsumeContext<MembershipCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var membership = context.Message.Packet.Membership;
                var user = context.Message.Packet.User;
                var complex = context.Message.Packet.Complex;

                membership.User = user;
                membership.Complex = complex;
                
                dbContext.AddRange(membership);

                dbContext.SaveChanges();
                
                return Task.CompletedTask;
            }
        }

        public Task Consume(ConsumeContext<SessionCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = context.Message.Packet.Session;
                var user = context.Message.Packet.BaseUser;

                session.BaseUser = dbContext.BaseUsers.Find(user.BaseUserId);
                
                dbContext.AddRange(session);

                dbContext.SaveChanges();
                
                return Task.CompletedTask;
            }
        }

        public Task Consume(ConsumeContext<UserProfileUpdatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalUser = context.Message.Packet.BaseUser;

                var localUser = dbContext.BaseUsers.Find(globalUser.BaseUserId);

                localUser.Title = globalUser.Title;
                localUser.Avatar = globalUser.Avatar;

                dbContext.SaveChanges();

                return Task.CompletedTask;
            }
        }

        public Task Consume(ConsumeContext<ComplexProfileUpdatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalComplex = context.Message.Packet.Complex;

                var localComplex = dbContext.Complexes.Find(globalComplex.ComplexId);

                localComplex.Title = globalComplex.Title;
                localComplex.Avatar = globalComplex.Avatar;

                dbContext.SaveChanges();

                return Task.CompletedTask;
            }
        }

        public Task Consume(ConsumeContext<ComplexDeletionNotif> context)
        {
            using (var dbContext = new DatabaseContext())
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
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoomProfileUpdatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalRoom = context.Message.Packet.Room;

                var localRoom = dbContext.Rooms.Find(globalRoom.RoomId);

                localRoom.Title = globalRoom.Title;
                localRoom.Avatar = globalRoom.Avatar;

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoomDeletionNotif> context)
        {
            using (var dbContext = new DatabaseContext())
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
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ContactCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var me = (User) dbContext.BaseUsers.Find(context.Message.Packet.Users[0].BaseUserId);
                var peer = (User) dbContext.BaseUsers.Find(context.Message.Packet.Users[1].BaseUserId);
                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;
                complexSecret.Complex = complex;
                complex.ComplexSecret = complexSecret;
                var room = context.Message.Packet.Room;
                room.Complex = complex;
                var m1 = context.Message.Packet.Memberships[0];
                m1.User = me;
                m1.Complex = complex;
                var m2 = context.Message.Packet.Memberships[1];
                m2.User = peer;
                m2.Complex = complex;
                var myContact = context.Message.Packet.Contacts[0];
                myContact.Complex = complex;
                myContact.User = me;
                myContact.Peer = peer;
                var peerContact = context.Message.Packet.Contacts[1];
                peerContact.Complex = complex;
                peerContact.User = peer;
                peerContact.Peer = me;
                dbContext.AddRange(complex, complexSecret, room, m1, m2, myContact, peerContact);
                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<InviteCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var invite = context.Message.Packet.Invite;
                var complex = dbContext.Complexes.Find(context.Message.Packet.Complex.ComplexId);
                var user = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);
                
                invite.Complex = complex;
                invite.User = user;
                
                dbContext.AddRange(invite);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<InviteCancelledNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var invite = dbContext.Invites.Find(context.Message.Packet.Invite.InviteId);
                var user = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);

                user.Invites.Remove(invite);
                dbContext.Invites.Remove(invite);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<InviteAcceptedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
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
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<InvitedIgnoredNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var invite = dbContext.Invites.Find(context.Message.Packet.Invite.InviteId);
                var human = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);
                
                dbContext.Entry(invite).Reference(i => i.Complex).Load();
                human.Invites.Remove(invite);
                dbContext.Invites.Remove(invite);
                
                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<SessionUpdatedNotif> context)
        {
            var globalSession = context.Message.Packet.Session;

            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(globalSession.SessionId);

                session.Online = globalSession.Online;
                session.ConnectionId = globalSession.ConnectionId;
                session.Token = globalSession.Token;

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }
    }
}