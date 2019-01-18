using System.Linq;
using System.Threading.Tasks;
using CityPlatform.DbContexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Internal;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Commands.User;
using SharedArea.Entities;
using SharedArea.Middles;

namespace CityPlatform.Consumers
{
    public class CityConsumer : IConsumer<CreateComplexRequest>, IConsumer<CreateRoomRequest>
        , IConsumer<CreateUserRequest>, IConsumer<CreateMembershipRequest>, IConsumer<CreateSessionRequest>
        , IConsumer<UpdateUserSecretRequest>, IConsumer<UpdateUserProfileRequest>, IConsumer<GetMeRequest>
        , IConsumer<GetUserByIdRequest>, IConsumer<SearchUsersRequest>
    {
        public async Task Consume(ConsumeContext<CreateComplexRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var admin = context.Message.Packet.User;
                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;
                var room = context.Message.Packet.Room;
                complex.ComplexSecret = complexSecret;
                complexSecret.Complex = complex;
                complexSecret.Admin = dbContext.BaseUsers.Find(admin.BaseUserId) as User;
                room.Complex = complex;

                dbContext.AddRange(complex, complexSecret, room);

                dbContext.SaveChanges();

                await context.RespondAsync<CreateComplexResponse>(new
                {
                    Packet = new Packet()
                    {
                        Complex = complex,
                        ComplexSecret = complexSecret,
                        Room = room
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<CreateRoomRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var complex = context.Message.Packet.Complex;
                var room = context.Message.Packet.Room;
                room.Complex = dbContext.Complexes.Find(complex.ComplexId);

                dbContext.AddRange(room);

                dbContext.SaveChanges();

                await context.RespondAsync<CreateRoomResponse>(new
                {
                    Packet = new Packet()
                    {
                        Complex = complex,
                        Room = room
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<CreateUserRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var user = context.Message.Packet.User;
                var userSecret = context.Message.Packet.UserSecret;

                user.UserSecret = userSecret;
                userSecret.User = user;
                
                dbContext.AddRange(user, userSecret);

                dbContext.SaveChanges();

                await context.RespondAsync<CreateUserResponse>(new
                {
                    Packet = new Packet()
                    {
                        User = user,
                        UserSecret = userSecret
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<CreateMembershipRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var membership = context.Message.Packet.Membership;
                var user = context.Message.Packet.User;
                var complex = context.Message.Packet.Complex;

                membership.User = dbContext.BaseUsers.Find(user.BaseUserId) as User;
                membership.Complex = dbContext.Complexes.Find(complex.ComplexId);
                
                dbContext.AddRange(membership);

                dbContext.SaveChanges();

                await context.RespondAsync<CreateMembershipResponse>(new
                {
                    Packet = new Packet()
                    {
                        Complex = complex,
                        User = user,
                        Membership = membership
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<CreateSessionRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = context.Message.Packet.Session;
                var user = context.Message.Packet.BaseUser;

                session.BaseUser = dbContext.BaseUsers.Find(user.BaseUserId);
                
                dbContext.AddRange(session);

                dbContext.SaveChanges();

                await context.RespondAsync<CreateSessionResponse>(new
                {
                    Packet = new Packet()
                    {
                        Session = session,
                        BaseUser = user
                    }
                });
            }
        }
        
        public async Task Consume(ConsumeContext<UpdateUserSecretRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalUs = context.Message.Packet.UserSecret;

                var us = dbContext.UserSecrets.Find(globalUs.UserSecretId);
                us.Email = globalUs.Email;
                us.Home = dbContext.Complexes.Find(globalUs.Home.ComplexId);
                us.User = dbContext.BaseUsers.Find(globalUs.UserId) as User;

                dbContext.SaveChanges();
                
                await context.RespondAsync<UpdateUserSecretResponse>(new
                {
                    Packet = new Packet()
                    {
                        UserSecret = us
                    }
                });
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
                
                SharedArea.Transport.NotifyService<UpdateUserProfileNotif>(
                    Program.Bus,
                    new Packet() {BaseUser = user},
                    SharedArea.GlobalVariables.AllQueuesExcept(new []
                    {
                        SharedArea.GlobalVariables.CITY_QUEUE_NAME
                    }));

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