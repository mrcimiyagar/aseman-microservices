using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MessengerPlatform.DbContexts;
using SharedArea.Commands.Internal.Notifications;

namespace MessengerPlatform.Consumers
{
    public class MessengerConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>, IConsumer<RoomCreatedNotif>
        , IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>, IConsumer<UserProfileUpdatedNotif>
        , IConsumer<ComplexProfileUpdatedNotif>, IConsumer<ComplexDeletionNotif>
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
                var admin = context.Message.Packet.User;
                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;
                
                complex.ComplexSecret = complexSecret;
                complexSecret.Complex = complex;
                complexSecret.Admin = admin;

                dbContext.AddRange(complex, complexSecret);

                dbContext.SaveChanges();
                
                return Task.CompletedTask;
            }
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
    }
}