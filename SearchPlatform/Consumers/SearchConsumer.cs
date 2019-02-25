using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchPlatform.DbContexts;
using SharedArea.Commands.Complex;
using SharedArea.Commands.Contact;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Room;
using SharedArea.Commands.User;
using SharedArea.Entities;
using SharedArea.Middles;

namespace SearchPlatform.Consumers
{
    public class SearchConsumer : IConsumer<SearchUsersRequest>, IConsumer<SearchComplexesRequest>
        , IConsumer<GetMeRequest>, IConsumer<GetUserByIdRequest>, IConsumer<GetComplexByIdRequest>
        , IConsumer<GetRoomByIdRequest>, IConsumer<GetRoomsRequest>, IConsumer<GetComplexesRequest>
        , IConsumer<GetContactsRequest>
        , IConsumer<ConsolidateDeleteAccountRequest>
    {
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
        
        public async Task Consume(ConsumeContext<SearchComplexesRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Query().Include(m => m.Complex).Load();
                var complexes = (from c in (from m in user.Memberships
                    where EF.Functions.Like(m.Complex.Title, "%" + packet.SearchQuery + "%")
                    select m) select c.Complex).ToList();

                await context.RespondAsync(new SearchComplexesResponse()
                {
                    Packet = new Packet {Status = "success", Complexes = complexes}
                });
            }
        }
        
        public async Task Consume(ConsumeContext<GetRoomByIdRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null)
                {
                    await context.RespondAsync(new GetRoomByIdResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }
                dbContext.Entry(membership).Reference(m => m.Complex).Load();
                dbContext.Entry(membership.Complex).Collection(c => c.Rooms).Load();
                var room = membership.Complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new GetRoomByIdResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                }
                else
                {
                    await context.RespondAsync(new GetRoomByIdResponse()
                    {
                        Packet = new Packet {Status = "success", Room = room}
                    });
                }
            }
        }
        
        public async Task Consume(ConsumeContext<GetComplexByIdRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var complex = dbContext.Complexes.Find(packet.Complex.ComplexId);
                if (complex == null)
                {
                    await context.RespondAsync(new GetComplexByIdResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }

                if (complex.Mode == 3)
                {
                    await context.RespondAsync(new GetComplexByIdResponse()
                    {
                        Packet = new Packet
                        {
                            Status = "success",
                            Complex = dbContext.Complexes.Find(packet.Complex.ComplexId)
                        }
                    });
                }
                else if (complex.Mode == 2)
                {
                    dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
                    dbContext.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new GetComplexByIdResponse()
                        {
                            Packet = new Packet {Status = "error_2"}
                        });
                    }
                    else
                    {
                        dbContext.Entry(membership).Reference(m => m.Complex).Load();
                        await context.RespondAsync(new GetComplexByIdResponse()
                        {
                            Packet = new Packet {Status = "success", Complex = membership.Complex}
                        });
                    }
                }
                else
                {
                    dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
                    dbContext.Entry(user).Reference(u => u.UserSecret).Load();
                    if (user.UserSecret.HomeId == packet.Complex.ComplexId)
                    {
                        dbContext.Entry(user.UserSecret).Reference(us => us.Home).Load();
                        await context.RespondAsync(new GetComplexByIdResponse()
                        {
                            Packet = new Packet { Status = "success", Complex = user.UserSecret.Home }
                        });
                    }
                    else
                    {
                        await context.RespondAsync(new GetComplexByIdResponse()
                        {
                            Packet = new Packet {Status = "error_3"}
                        });
                    }
                }
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
        
        public async Task Consume(ConsumeContext<GetRoomsRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                if (packet.Complex.ComplexId > 0)
                {
                    var membership = user.Memberships.Find(mem => mem.ComplexId == packet.Complex.ComplexId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new GetRoomsResponse()
                        {
                            Packet = new Packet {Status = "error_0B0"}
                        });
                        return;
                    }

                    dbContext.Entry(membership).Reference(mem => mem.Complex).Load();
                    dbContext.Entry(membership.Complex).Collection(c => c.Rooms).Load();
                    await context.RespondAsync(new GetRoomsResponse()
                    {
                        Packet = new Packet {Status = "success", Rooms = membership.Complex.Rooms}
                    });
                }
                else
                {
                    var rooms = new List<Room>();
                    foreach (var membership in user.Memberships)
                    {
                        dbContext.Entry(membership).Reference(m => m.Complex).Query().Include(c => c.Rooms).Load();
                        rooms.AddRange(membership.Complex.Rooms);
                    }
                    await context.RespondAsync(new GetRoomsResponse()
                    {
                        Packet = new Packet {Status = "success", Rooms = rooms}
                    });
                }
            }
        }
        
        public async Task Consume(ConsumeContext<GetComplexesRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Query().Include(m => m.Complex).Load();
                var complexes = user.Memberships.Select(m => m.Complex).ToList();
                var complexSecrets = new List<ComplexSecret>();
                foreach (var complex in complexes)
                {
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    dbContext.Entry(complex).Collection(c => c.Members).Query().Include(m => m.User).Load();
                    dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                    if (complex.ComplexSecret == null) continue;
                    if (complex.ComplexSecret.AdminId == user.BaseUserId)
                        complexSecrets.Add(complex.ComplexSecret);
                }
                await context.RespondAsync(new GetComplexesResponse()
                {
                    Packet = new Packet {Status = "success", Complexes = complexes, ComplexSecrets = complexSecrets}
                });
            }
        }

        public async Task Consume(ConsumeContext<GetContactsRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Contacts).Load();
                var contacts = user.Contacts;
                foreach (var contact in contacts)
                {
                    dbContext.Entry(contact).Reference(c => c.Complex).Load();
                    dbContext.Entry(contact.Complex).Collection(c => c.Rooms).Load();
                    dbContext.Entry(contact).Reference(c => c.Peer).Load();
                    dbContext.Entry(contact).Reference(c => c.User).Load();
                }

                await context.RespondAsync(new GetContactsResponse()
                {
                    Packet = new Packet {Status = "success", Contacts = contacts}
                });
            }
        }

        public async Task Consume(ConsumeContext<ConsolidateDeleteAccountRequest> context)
        {
            var gUser = context.Message.Packet.User;

            using (var dbContext = new DatabaseContext())
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

            await context.RespondAsync(new ConsolidateDeleteAccountResponse());
        }
    }
}