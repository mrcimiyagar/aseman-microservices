using System.Threading.Tasks;
using DesktopPlatform.DbContexts;
using MassTransit;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Entities;

namespace DesktopPlatform.Consumers
{
    public class DesktopConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>, IConsumer<RoomCreatedNotif>
        , IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>
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
    }
}