using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityPlatform.DbContexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Complex;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Commands.Pushes;
using SharedArea.Commands.Room;
using SharedArea.Commands.User;
using SharedArea.Consumers;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;

namespace CityPlatform.Consumers
{
    public class CityConsumer : NotifConsumer, IConsumer<PutComplexRequest>, IConsumer<PutRoomRequest>
        , IConsumer<PutUserRequest>, IConsumer<PutMembershipRequest>, IConsumer<PutSessionRequest>
        , IConsumer<UpdateUserSecretRequest>, IConsumer<UpdateUserProfileRequest>, IConsumer<GetMeRequest>
        , IConsumer<GetUserByIdRequest>, IConsumer<SearchUsersRequest>, IConsumer<UpdateComplexProfileRequest>
        , IConsumer<CreateComplexRequest>, IConsumer<DeleteComplexRequest>, IConsumer<GetComplexesRequest>
        , IConsumer<GetComplexByIdRequest>, IConsumer<SearchComplexesRequest>, IConsumer<UpdateRoomProfileRequest>
        , IConsumer<DeleteRoomRequest>, IConsumer<GetRoomsRequest>, IConsumer<GetRoomByIdRequest>
    {
        public async Task Consume(ConsumeContext<PutComplexRequest> context)
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

                await context.RespondAsync<PutComplexResponse>(new
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

        public async Task Consume(ConsumeContext<PutRoomRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var complex = context.Message.Packet.Complex;
                var room = context.Message.Packet.Room;
                room.Complex = dbContext.Complexes.Find(complex.ComplexId);

                dbContext.AddRange(room);

                dbContext.SaveChanges();

                await context.RespondAsync<PutRoomResponse>(new
                {
                    Packet = new Packet()
                    {
                        Complex = complex,
                        Room = room
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<PutUserRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var user = context.Message.Packet.User;
                var userSecret = context.Message.Packet.UserSecret;

                user.UserSecret = userSecret;
                userSecret.User = user;
                
                dbContext.AddRange(user, userSecret);

                dbContext.SaveChanges();

                await context.RespondAsync<PutUserResponse>(new
                {
                    Packet = new Packet()
                    {
                        User = user,
                        UserSecret = userSecret
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<PutMembershipRequest> context)
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

                await context.RespondAsync<PutMembershipResponse>(new
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

        public async Task Consume(ConsumeContext<PutSessionRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = context.Message.Packet.Session;
                var user = context.Message.Packet.BaseUser;

                session.BaseUser = dbContext.BaseUsers.Find(user.BaseUserId);
                
                dbContext.AddRange(session);

                dbContext.SaveChanges();

                await context.RespondAsync<PutSessionResponse>(new
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
                
                SharedArea.Transport.NotifyService<UserProfileUpdatedNotif>(
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

        public async Task Consume(ConsumeContext<UpdateComplexProfileRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var complex = dbContext.Complexes.Include(c => c.ComplexSecret)
                    .SingleOrDefault(c => c.ComplexId == packet.Complex.ComplexId);
                if (complex?.ComplexSecret.AdminId == session.BaseUser.BaseUserId)
                {
                    complex.Title = packet.Complex.Title;
                    complex.Avatar = packet.Complex.Avatar;
                    dbContext.SaveChanges();
                    
                    SharedArea.Transport.NotifyService<ComplexProfileUpdatedNotif>(
                        Program.Bus,
                        new Packet() {Complex = complex},
                        SharedArea.GlobalVariables.AllQueuesExcept(new []
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME
                        }));

                    await context.RespondAsync(new UpdateComplexProfileResponse()
                    {
                        Packet = new Packet {Status = "success", Complex = complex}
                    });
                }
                else
                {
                    await context.RespondAsync(new UpdateComplexProfileResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<CreateComplexRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                if (!string.IsNullOrEmpty(packet.Complex.Title) && packet.Complex.Title.ToLower() != "home")
                {
                    dbContext.Entry(user).Reference(u => u.UserSecret).Load();
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
                    dbContext.AddRange(complex, cs, room, mem);
                    dbContext.SaveChanges();
                    
                    SharedArea.Transport.NotifyService<ComplexCreatedNotif>(
                        Program.Bus,
                        new Packet() {User = user, Complex = complex, ComplexSecret = cs, Room = room},
                        SharedArea.GlobalVariables.AllQueuesExcept(new []
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME
                        }));
                    
                    SharedArea.Transport.NotifyService<MembershipCreatedNotif>(
                        Program.Bus,
                        new Packet() {Membership = mem, User = user, Complex = complex},
                        SharedArea.GlobalVariables.AllQueuesExcept(new []
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME
                        }));
                        
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();

                    await context.RespondAsync(new CreateComplexResponse()
                    {
                        Packet = new Packet {Status = "success", Complex = complex}
                    });
                }
                else
                {
                    await context.RespondAsync(new CreateComplexResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<DeleteComplexRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var complex = dbContext.Complexes.Find(packet.Complex.ComplexId);
                if (complex != null)
                {
                    dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                    if (complex.ComplexSecret.AdminId == session.BaseUserId)
                    {
                        if (complex.Title != "" && complex.Title.ToLower() != "home")
                        {
                            dbContext.Entry(complex).Collection(c => c.Members).Load();
                            var members = complex.Members.ToList();
                            foreach (var membership in members)
                            {
                                dbContext.Entry(membership).Reference(m => m.User).Load();
                                var user = membership.User;
                                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                                user.Memberships.Remove(membership);
                                dbContext.Memberships.Remove(membership);

                                var sessionIds = new List<long>();
                                foreach (var memSess in user.Sessions)
                                {
                                    sessionIds.Add(memSess.SessionId);
                                }

                                var cdn = new ComplexDeletionNotification()
                                {
                                    ComplexId = complex.ComplexId
                                };
                                var push = new ComplexDeletionPush()
                                {
                                    SessionIds = sessionIds,
                                    Notif = cdn
                                };

                                SharedArea.Transport.Push<ComplexDeletionPush>(
                                    Program.Bus,
                                    push);
                            }

                            dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                            foreach (var room in complex.Rooms)
                            {
                                dbContext.Rooms.Remove(room);
                            }

                            dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                            dbContext.ComplexSecrets.Remove(complex.ComplexSecret);
                            dbContext.Complexes.Remove(complex);

                            dbContext.SaveChanges();

                            SharedArea.Transport.NotifyService<ComplexDeletionNotif>(
                                Program.Bus,
                                new Packet() {Complex = new Complex() {ComplexId = complex.ComplexId}},
                                SharedArea.GlobalVariables.AllQueuesExcept(new[]
                                {
                                    SharedArea.GlobalVariables.CITY_QUEUE_NAME
                                }));

                            await context.RespondAsync(new DeleteComplexResponse()
                            {
                                Packet = new Packet {Status = "success"}
                            });
                        }
                        else
                        {
                            await context.RespondAsync(new DeleteComplexResponse()
                            {
                                Packet = new Packet {Status = "error_0"}
                            });
                        }
                    }
                    else
                    {
                        await context.RespondAsync(new DeleteComplexResponse()
                        {
                            Packet = new Packet {Status = "error_2"}
                        });
                    }
                }
                else
                {
                    await context.RespondAsync(new DeleteComplexResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
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

                await context.RespondAsync(new GetComplexesResponse()
                {
                    Packet = new Packet {Status = "success", Complexes = complexes}
                });
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
                    await context.RespondAsync(new GetComplexesResponse()
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

        public async Task Consume(ConsumeContext<UpdateRoomProfileRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var complex = dbContext.Complexes.Find(packet.Room.ComplexId);
                dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                if (complex.ComplexSecret.AdminId == session.BaseUser.BaseUserId)
                {
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    if (complex.Rooms.Any(r => r.RoomId == packet.Room.RoomId))
                    {
                        var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                        room.Title = packet.Room.Title;
                        room.Avatar = packet.Room.Avatar;
                        dbContext.SaveChanges();
                        
                        SharedArea.Transport.NotifyService<RoomProfileUpdatedNotif>(
                            Program.Bus,
                            new Packet() {Room = room},
                            SharedArea.GlobalVariables.AllQueuesExcept(new []
                            {
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME
                            }));

                        await context.RespondAsync(new UpdateRoomProfileResponse()
                        {
                            Packet = new Packet { Status = "success" }
                        });
                    }
                    else
                    {
                        await context.RespondAsync(new UpdateRoomProfileResponse()
                        {
                            Packet = new Packet { Status = "error_0" }
                        });
                    }
                }
                else
                {
                    await context.RespondAsync(new UpdateRoomProfileResponse()
                    {
                        Packet = new Packet { Status = "error_1" }
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<DeleteRoomRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                dbContext.Entry(human).Collection(u => u.Memberships).Load();
                if (human.Memberships.Any(m => m.ComplexId == packet.Room.ComplexId)) 
                {
                    var complex = dbContext.Complexes.Find(packet.Room.ComplexId);
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    if (complex.Rooms.Any(r => r.RoomId == packet.Room.RoomId))
                    {
                        var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                        complex.Rooms.Remove(room);
                        dbContext.Rooms.Remove(room);
                        dbContext.SaveChanges();
                        
                        SharedArea.Transport.NotifyService<RoomDeletionNotif>(
                            Program.Bus,
                            new Packet() {Room = room},
                            SharedArea.GlobalVariables.AllQueuesExcept(new []
                            {
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME
                            }));
                        
                        dbContext.Entry(complex).Collection(c => c.Members).Load();
                        var sessionIds = new List<long>();
                        foreach (var ms in complex.Members)
                        {
                            try
                            {
                                dbContext.Entry(ms).Reference(mem => mem.User).Load();
                                dbContext.Entry(ms.User).Collection(u => u.Sessions).Load();
                                foreach (var userSession in ms.User.Sessions)
                                {
                                    sessionIds.Add(userSession.SessionId);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        
                        var rdn = new RoomDeletionNotification()
                        {
                            ComplexId = complex.ComplexId,
                            RoomId = room.RoomId,
                        };
                        
                        SharedArea.Transport.Push<RoomDeletionPush>(
                            Program.Bus,
                            new RoomDeletionPush()
                            {
                                SessionIds = sessionIds,
                                Notif = rdn
                            });

                        await context.RespondAsync(new DeleteRoomResponse()
                        {
                            Packet = new Packet { Status = "success" }
                        });
                    }
                    else
                    {
                        await context.RespondAsync(new DeleteRoomResponse()
                        {
                            Packet = new Packet { Status = "error_0" }
                        });
                    }
                }
                else
                {
                    await context.RespondAsync(new DeleteRoomResponse()
                    {
                        Packet = new Packet { Status = "error_1" }
                    });
                }
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
                    await context.RespondAsync(new GetRoomByIdRequest()
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
                    await context.RespondAsync(new GetRoomByIdRequest()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                }
                else
                {
                    await context.RespondAsync(new GetRoomByIdRequest()
                    {
                        Packet = new Packet {Status = "success", Room = room}
                    });
                }
            }
        }
    }
}