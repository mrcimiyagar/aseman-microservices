using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProfilePlatform.DbContexts;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.User;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Utils;

namespace ProfilePlatform.Consumers
{
    public class ProfileConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>, IConsumer<RoomCreatedNotif>
        , IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>, IConsumer<UpdateUserProfileRequest>
        , IConsumer<GetMeRequest>, IConsumer<GetUserByIdRequest>, IConsumer<SearchUsersRequest>
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

        public async Task Consume(ConsumeContext<UpdateUserProfileRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = session.BaseUser;
                user.Title = packet.User.Title;
                user.Avatar = packet.User.Avatar;
                dbContext.SaveChanges();

                await context.RespondAsync(new UpdateUserProfileResponse()
                {
                    Packet = new Packet()
                    {
                        Status = "success"
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<GetMeRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Reference(u => u.UserSecret).Load();
                dbContext.Entry(user.UserSecret).Reference(us => us.Home).Load();

                await context.RespondAsync(new GetMeResponse()
                {
                    Packet = new Packet
                    {
                        Status = "success",
                        User = user,
                        UserSecret = user.UserSecret,
                        Complex = user.UserSecret.Home
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<GetUserByIdRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var user = dbContext.BaseUsers.Find(packet.BaseUser.BaseUserId);

                await context.RespondAsync(new GetUserByIdResponse()
                {
                    Packet = new Packet {Status = "success", BaseUser = user}
                });
            }
        }

        public async Task Consume(ConsumeContext<SearchUsersRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var users = (from u in dbContext.Users
                    where EF.Functions.Like(u.Title, "%" + packet.SearchQuery + "%")
                    select u).ToList();

                await context.RespondAsync(new SearchUsersResponse()
                {
                    Packet = new Packet {Status = "success", Users = users}
                });
            }
        }
    }
}