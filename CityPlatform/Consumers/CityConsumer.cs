using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CityPlatform.DbContexts;
using CityPlatform.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Clauses;
using SharedArea.Commands.Bot;
using SharedArea.Commands.Complex;
using SharedArea.Commands.Contact;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Commands.Invite;
using SharedArea.Commands.Pushes;
using SharedArea.Commands.Room;
using SharedArea.Commands.User;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;

namespace CityPlatform.Consumers
{
    public class CityConsumer : IConsumer<PutComplexRequest>, IConsumer<PutRoomRequest>
        , IConsumer<PutUserRequest>, IConsumer<PutMembershipRequest>, IConsumer<PutSessionRequest>
        , IConsumer<UpdateUserSecretRequest>, IConsumer<UpdateUserProfileRequest>
        , IConsumer<UpdateComplexProfileRequest>, IConsumer<MakeAccountRequest>
        , IConsumer<CreateComplexRequest>, IConsumer<DeleteComplexRequest>, IConsumer<UpdateRoomProfileRequest>
        , IConsumer<DeleteRoomRequest>, IConsumer<CreateContactRequest>, IConsumer<CreateInviteRequest>
        , IConsumer<CancelInviteRequest>, IConsumer<AcceptInviteRequest>, IConsumer<IgnoreInviteRequest>
        , IConsumer<GetBotsRequest>, IConsumer<GetCreatedBotsRequest>, IConsumer<GetSubscribedBotsRequest>
        , IConsumer<SearchBotsRequest>, IConsumer<UpdateBotProfileRequest>, IConsumer<GetBotRequest>
        , IConsumer<SubscribeBotRequest>, IConsumer<CreateBotRequest>, IConsumer<CreateRoomRequest>
        , IConsumer<ConsolidateDeleteAccountRequest>, IConsumer<GetMyInvitesRequest>, IConsumer<UpdateMemberAccessRequest>
        , IConsumer<GetComplexAccessesRequest>
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

                if (packet.User.Title.Length == 0)
                {
                    await context.RespondAsync(new UpdateUserProfileResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }

                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = session.BaseUser;
                user.Title = packet.User.Title;
                user.Avatar = packet.User.Avatar;
                dbContext.SaveChanges();

                SharedArea.Transport.NotifyService<UserProfileUpdatedNotif, UserProfileUpdatedNotifResponse>(
                    Program.Bus,
                    new Packet() {BaseUser = user},
                    SharedArea.GlobalVariables.AllQueuesExcept(new[]
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

        public async Task Consume(ConsumeContext<UpdateComplexProfileRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var complex = dbContext.Complexes.Include(c => c.ComplexSecret)
                    .SingleOrDefault(c => c.ComplexId == packet.Complex.ComplexId);
                if (complex == null)
                {
                    await context.RespondAsync(new UpdateComplexProfileResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }

                if (complex.Title.ToLower() != "home" && complex.ComplexSecret.AdminId == session.BaseUser.BaseUserId)
                {
                    complex.Title = packet.Complex.Title;
                    complex.Avatar = packet.Complex.Avatar;
                    dbContext.SaveChanges();

                    SharedArea.Transport.NotifyService<ComplexProfileUpdatedNotif, ComplexProfileUpdatedNotifResponse>(
                        Program.Bus,
                        new Packet() {Complex = complex},
                        SharedArea.GlobalVariables.AllQueuesExcept(new[]
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
                        Mode = 3,
                        ComplexSecret = new ComplexSecret()
                        {
                            Admin = user
                        },
                        Rooms = new List<Room>()
                        {
                            new Room()
                            {
                                Title = "Hall",
                                Avatar = 0,
                            }
                        },
                        Members = new List<Membership>()
                        {
                            new Membership()
                            {
                                User = user,
                                MemberAccess = new MemberAccess()
                                {
                                    CanCreateMessage = true,
                                    CanModifyAccess = true,
                                    CanModifyWorkers = true,
                                    CanSendInvite = true,
                                    CanUpdateProfiles = true
                                }
                            }
                        }
                    };
                    complex.ComplexSecret.Complex = complex;
                    complex.Rooms[0].Complex = complex;
                    complex.Members[0].Complex = complex;
                    complex.Members[0].MemberAccess.Membership = complex.Members[0];

                    dbContext.AddRange(complex);
                    dbContext.SaveChanges();

                    SharedArea.Transport.NotifyService<ComplexCreatedNotif, ComplexCreatedNotifResponse>(
                        Program.Bus,
                        new Packet() {User = user, Complex = complex, ComplexSecret = complex.ComplexSecret},
                        SharedArea.GlobalVariables.AllQueuesExcept(new[]
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                            SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME
                        }));

                    await SharedArea.Transport
                        .RequestService<ConsolidateCreateComplexRequest, ConsolidateCreateComplexResponse>(
                            Program.Bus,
                            SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                            new Packet() {User = user, Complex = complex, ComplexSecret = complex.ComplexSecret});

                    var message = new ServiceMessage()
                    {
                        Author = null,
                        Room = complex.Rooms[0],
                        RoomId = complex.Rooms[0].RoomId,
                        Text = "Room created.",
                        Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    var result = await SharedArea.Transport
                        .RequestService<PutServiceMessageRequest, PutServiceMessageResponse>(
                            Program.Bus,
                            SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                            new Packet() {ServiceMessage = message});

                    message.MessageId = result.Packet.ServiceMessage.MessageId;

                    await context.RespondAsync(new CreateComplexResponse()
                    {
                        Packet = new Packet
                        {
                            Status = "success",
                            Complex = complex,
                            ComplexSecret = complex.ComplexSecret,
                            ServiceMessage = message
                        }
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
                            dbContext.Entry(complex).Collection(c => c.Members).Query()
                                .Include(mem => mem.MemberAccess)
                                .Include(mem => mem.User)
                                .ThenInclude(u => u.Sessions)
                                .Load();
                            var sessionIds =
                                (from m in complex.Members
                                    where m.UserId == session.BaseUserId
                                    from s in m.User.Sessions
                                    select s.SessionId).ToList();
                            var members = complex.Members.ToList();
                            foreach (var membership in members)
                            {
                                dbContext.MemberAccesses.Remove(membership.MemberAccess);
                                dbContext.Entry(membership).Reference(m => m.User).Load();
                                var user = membership.User;
                                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                                user.Memberships.Remove(membership);
                                dbContext.Memberships.Remove(membership);
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

                            dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                            foreach (var room in complex.Rooms)
                            {
                                dbContext.Rooms.Remove(room);
                            }

                            dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                            dbContext.ComplexSecrets.Remove(complex.ComplexSecret);
                            dbContext.Complexes.Remove(complex);

                            dbContext.SaveChanges();

                            SharedArea.Transport.NotifyService<ComplexDeletionNotif, ComplexDeletionNotifResponse>(
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

                        SharedArea.Transport.NotifyService<RoomProfileUpdatedNotif, RoomProfileUpdatedNotifResponse>(
                            Program.Bus,
                            new Packet() {Room = room},
                            SharedArea.GlobalVariables.AllQueuesExcept(new[]
                            {
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME
                            }));

                        await context.RespondAsync(new UpdateRoomProfileResponse()
                        {
                            Packet = new Packet {Status = "success"}
                        });
                    }
                    else
                    {
                        await context.RespondAsync(new UpdateRoomProfileResponse()
                        {
                            Packet = new Packet {Status = "error_0"}
                        });
                    }
                }
                else
                {
                    await context.RespondAsync(new UpdateRoomProfileResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
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
                    if (complex.Rooms.Count > 1 && complex.Rooms.Any(r => r.RoomId == packet.Room.RoomId))
                    {
                        var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                        complex.Rooms.Remove(room);
                        dbContext.Rooms.Remove(room);
                        dbContext.SaveChanges();

                        SharedArea.Transport.NotifyService<RoomDeletionNotif, RoomDeletionNotifResponse>(
                            Program.Bus,
                            new Packet() {Room = room},
                            SharedArea.GlobalVariables.AllQueuesExcept(new[]
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
                            Packet = new Packet {Status = "success"}
                        });
                    }
                    else
                    {
                        await context.RespondAsync(new DeleteRoomResponse()
                        {
                            Packet = new Packet {Status = "error_0"}
                        });
                    }
                }
                else
                {
                    await context.RespondAsync(new DeleteRoomResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<CreateContactRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var me = (User) session.BaseUser;
                var peer = (User) dbContext.BaseUsers.Find(packet.User.BaseUserId);

                if (me.BaseUserId == peer.BaseUserId)
                {
                    await context.RespondAsync(new CreateContactResponse()
                    {
                        Packet = new Packet
                        {
                            Status = "error_1",
                        }
                    });
                    return;
                }

                dbContext.Entry(me).Collection(u => u.Contacts).Load();

                if (me.Contacts.All(c => c.PeerId != packet.User.BaseUserId))
                {
                    var complex = new Complex
                    {
                        Title = "",
                        Avatar = -1,
                        Mode = 2
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
                        Complex = complex,
                        MemberAccess = new MemberAccess()
                        {
                            CanCreateMessage = true,
                            CanModifyAccess = false,
                            CanModifyWorkers = true,
                            CanSendInvite = true,
                            CanUpdateProfiles = true
                        }
                    };
                    var m2 = new Membership()
                    {
                        User = peer,
                        Complex = complex,
                        MemberAccess = new MemberAccess()
                        {
                            CanCreateMessage = true,
                            CanModifyAccess = false,
                            CanModifyWorkers = true,
                            CanSendInvite = true,
                            CanUpdateProfiles = true
                        }
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
                    m1.MemberAccess.Membership = m1;
                    m2.MemberAccess.Membership = m2;
                    dbContext.AddRange(complex, complexSecret, room, m1, m2, myContact, peerContact);
                    dbContext.SaveChanges();

                    SharedArea.Transport.NotifyService<ContactCreatedNotif, ContactCreatedNotifResponse>(
                        Program.Bus,
                        new Packet()
                        {
                            Complex = complex,
                            ComplexSecret = complexSecret,
                            Room = room,
                            Memberships = new[] {m1, m2}.ToList(),
                            Contacts = new[] {myContact, peerContact}.ToList(),
                            Users = new[] {me, peer}.ToList()
                        },
                        SharedArea.GlobalVariables.AllQueuesExcept(new[]
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                            SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME
                        }));

                    await SharedArea.Transport.RequestService<ConsolidateContactRequest, ConsolidateContactResponse>(
                        Program.Bus,
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                        new Packet()
                        {
                            Complex = complex,
                            ComplexSecret = complexSecret,
                            Room = room,
                            Memberships = new[] {m1, m2}.ToList(),
                            Contacts = new[] {myContact, peerContact}.ToList(),
                            Users = new[] {me, peer}.ToList()
                        });

                    var message = new ServiceMessage
                    {
                        Text = "Room created.",
                        Room = room,
                        Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Author = null
                    };

                    var result1 = await SharedArea.Transport
                        .RequestService<PutServiceMessageRequest, PutServiceMessageResponse>(
                            Program.Bus,
                            SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                            new Packet() {ServiceMessage = message});

                    message.MessageId = result1.Packet.ServiceMessage.MessageId;

                    dbContext.Messages.Add(message);
                    dbContext.SaveChanges();

                    dbContext.Entry(peerContact.Complex).Collection(c => c.Rooms).Load();

                    dbContext.Entry(peer).Collection(u => u.Sessions).Load();
                    var sessionIds = peer.Sessions.Select(s => s.SessionId).ToList();
                    
                    ServiceMessage finalMessage;
                    Contact finalMyContact;
                    using (var finalContext = new DatabaseContext())
                    {
                        finalMessage = (ServiceMessage) finalContext.Messages.Find(message.MessageId);
                        finalMyContact = finalContext.Contacts.Find(myContact.ContactId);
                        finalContext.Entry(finalMessage).Reference(m => m.Room).Load();
                        finalContext.Entry(finalMessage.Room).Reference(r => r.Complex).Load();
                        finalContext.Entry(finalMyContact).Reference(c => c.Complex).Load();
                        finalContext.Entry(finalMyContact.Complex).Collection(c => c.Rooms).Load();
                        finalContext.Entry(finalMyContact).Reference(c => c.User).Load();
                        finalContext.Entry(finalMyContact).Reference(c => c.Peer).Load();
                        finalContext.Entry(finalMyContact.Complex).Collection(c => c.Members)
                            .Query().Include(m => m.User).Include(m => m.MemberAccess).Load();
                        
                        message = (ServiceMessage) finalContext.Messages.Find(message.MessageId);
                        peerContact = finalContext.Contacts.Find(peerContact.ContactId);
                        finalContext.Entry(message).Reference(m => m.Room).Load();
                        finalContext.Entry(message.Room).Reference(r => r.Complex).Load();
                        finalContext.Entry(peerContact).Reference(c => c.Complex).Load();
                        finalContext.Entry(peerContact.Complex).Collection(c => c.Rooms).Load();
                        finalContext.Entry(peerContact).Reference(c => c.User).Load();
                        finalContext.Entry(peerContact).Reference(c => c.Peer).Load();
                        finalContext.Entry(peerContact.Complex).Collection(c => c.Members)
                            .Query().Include(m => m.User).Include(m => m.MemberAccess).Load();
                    }

                    var ccn = new ContactCreationNotification
                    {
                        Contact = peerContact,
                        ComplexSecret = complexSecret
                    };
                    var mcn = new ServiceMessageNotification
                    {
                        Message = message
                    };

                    SharedArea.Transport.Push<ContactCreationPush>(
                        Program.Bus,
                        new ContactCreationPush()
                        {
                            Notif = ccn,
                            SessionIds = sessionIds
                        });

                    SharedArea.Transport.Push<ServiceMessagePush>(
                        Program.Bus,
                        new ServiceMessagePush()
                        {
                            Notif = mcn,
                            SessionIds = sessionIds
                        });

                    await context.RespondAsync(new CreateContactResponse()
                    {
                        Packet = new Packet
                        {
                            Status = "success",
                            Contact = finalMyContact,
                            ServiceMessage = finalMessage,
                            ComplexSecret = complexSecret
                        }
                    });
                }
                else
                {
                    await context.RespondAsync(new CreateContactResponse()
                    {
                        Packet = new Packet {Status = "error_050"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<CreateInviteRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var human = dbContext.Users.Find(packet.User.BaseUserId);
                dbContext.Entry(human).Collection(h => h.Memberships).Load();

                if (human.Memberships.Any(m => m.ComplexId == packet.Complex.ComplexId))
                {
                    await context.RespondAsync(new CreateInviteResponse()
                    {
                        Packet = new Packet {Status = "error_0H2"}
                    });
                    return;
                }

                dbContext.Entry(human).Collection(h => h.Invites).Load();
                if (human.Invites.Any(i => i.ComplexId == packet.Complex.ComplexId))
                {
                    await context.RespondAsync(new CreateInviteResponse()
                    {
                        Packet = new Packet {Status = "error_0H1"}
                    });
                    return;
                }

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var me = (User) session.BaseUser;
                dbContext.Entry(me).Collection(u => u.Memberships).Load();
                var mem = me.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (mem == null)
                {
                    await context.RespondAsync(new CreateInviteResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }

                dbContext.Entry(mem).Reference(m => m.Complex).Load();
                var complex = mem.Complex;
                if (complex.Mode == 1 || complex.Mode == 2)
                {
                    await context.RespondAsync(new CreateInviteResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }
                
                dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                dbContext.Entry(mem).Reference(m => m.MemberAccess).Load();
                if (mem.MemberAccess.CanSendInvite)
                {
                    var invite = new Invite()
                    {
                        Complex = complex,
                        User = human
                    };
                    dbContext.AddRange(invite);
                    dbContext.SaveChanges();

                    SharedArea.Transport.NotifyService<InviteCreatedNotif, InviteCreatedNotifResponse>(
                        Program.Bus,
                        new Packet() {Invite = invite, Complex = complex, User = human},
                        SharedArea.GlobalVariables.AllQueuesExcept(new[]
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME
                        }));

                    dbContext.Entry(human).Collection(h => h.Sessions).Load();
                    var sessionIds = new List<long>();
                    foreach (var targetSession in human.Sessions)
                    {
                        sessionIds.Add(targetSession.SessionId);
                    }

                    var inviteNotification = new InviteCreationNotification()
                    {
                        Invite = invite
                    };

                    SharedArea.Transport.Push<InviteCreationPush>(
                        Program.Bus,
                        new InviteCreationPush()
                        {
                            SessionIds = sessionIds,
                            Notif = inviteNotification
                        });

                    await context.RespondAsync(new CreateInviteResponse()
                    {
                        Packet = new Packet {Status = "success", Invite = invite}
                    });
                }
                else
                {
                    await context.RespondAsync(new CreateInviteResponse()
                    {
                        Packet = new Packet {Status = "error_0H0"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<CancelInviteRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                var mem = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (mem == null)
                {
                    await context.RespondAsync(new CancelInviteResponse()
                    {
                        Packet = new Packet {Status = "error_3"}
                    });
                    return;
                }

                dbContext.Entry(mem).Reference(m => m.Complex).Load();
                var complex = mem.Complex;
                if (complex != null)
                {
                    dbContext.Entry(complex).Collection(c => c.Invites).Load();
                    var invite = complex.Invites.Find(i => i.UserId == packet.User.BaseUserId);
                    if (invite != null)
                    {
                        dbContext.Entry(invite).Reference(i => i.User).Load();
                        var human = invite.User;
                        dbContext.Entry(human).Collection(h => h.Invites).Load();
                        human.Invites.Remove(invite);
                        dbContext.Invites.Remove(invite);
                        dbContext.SaveChanges();

                        SharedArea.Transport.NotifyService<InviteCancelledNotif, InviteCancelledNotifResponse>(
                            Program.Bus,
                            new Packet() {User = human, Invite = invite},
                            SharedArea.GlobalVariables.AllQueuesExcept(new[]
                            {
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME
                            }));

                        dbContext.Entry(human).Collection(h => h.Sessions).Load();
                        var sessionIds = human.Sessions.Select(s => s.SessionId).ToList();
                        var notification = new InviteCancellationNotification
                        {
                            Invite = invite
                        };

                        SharedArea.Transport.Push<InviteCancellationPush>(
                            Program.Bus,
                            new InviteCancellationPush()
                            {
                                Notif = notification,
                                SessionIds = sessionIds
                            });

                        await context.RespondAsync(new CancelInviteResponse()
                        {
                            Packet = new Packet {Status = "success"}
                        });
                    }
                    else
                    {
                        await context.RespondAsync(new CancelInviteResponse()
                        {
                            Packet = new Packet {Status = "error_0I0"}
                        });
                    }
                }
                else
                {
                    await context.RespondAsync(new CancelInviteResponse()
                    {
                        Packet = new Packet {Status = "error_0I2"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<AcceptInviteRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                dbContext.Entry(human).Collection(h => h.Invites).Load();
                var invite = human.Invites.Find(i => i.ComplexId == packet.Complex.ComplexId);
                if (invite != null)
                {
                    dbContext.Entry(invite).Reference(i => i.Complex).Load();
                    var complex = invite.Complex;
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    dbContext.Entry(complex).Collection(c => c.Members).Query().Include(m => m.User).Load();
                    human.Invites.Remove(invite);
                    dbContext.Invites.Remove(invite);
                    var membership = new Membership
                    {
                        User = human,
                        Complex = complex,
                        MemberAccess = new MemberAccess()
                        {
                            CanCreateMessage = true,
                            CanModifyAccess = false,
                            CanModifyWorkers = false,
                            CanSendInvite = false,
                            CanUpdateProfiles = false
                        }
                    };
                    membership.MemberAccess.Membership = membership;

                    dbContext.Entry(complex).Collection(c => c.Members).Load();
                    complex.Members.Add(membership);
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    var hall = complex.Rooms.FirstOrDefault();

                    dbContext.SaveChanges();

                    membership = dbContext.Memberships.Find(membership.MembershipId);
                    dbContext.Entry(membership).Reference(m => m.MemberAccess).Load();
                    dbContext.Entry(membership).Reference(m => m.User).Load();
                    dbContext.Entry(membership).Reference(m => m.Complex).Query()
                        .Include(c => c.Rooms).Include(c => c.Members)
                        .ThenInclude(m => m.User).Load();

                    SharedArea.Transport.NotifyService<InviteAcceptedNotif, InviteAcceptedNotifResponse>(
                        Program.Bus,
                        new Packet() {Invite = invite, Membership = membership, User = human},
                        SharedArea.GlobalVariables.AllQueuesExcept(new[]
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME
                        }));

                    var message = new ServiceMessage()
                    {
                        Room = hall,
                        Author = null,
                        Text = human.Title + " entered complex by invite.",
                        Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    var result = await SharedArea.Transport
                        .RequestService<PutServiceMessageRequest, PutServiceMessageResponse>(
                            Program.Bus,
                            SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                            new Packet() {ServiceMessage = message, Room = hall});

                    message = result.Packet.ServiceMessage;

                    User user;
                    Complex jointComplex;
                    using (var context2 = new DatabaseContext())
                    {
                        user = context2.Users.Find(human.BaseUserId);
                        jointComplex = context2.Complexes.Find(complex.ComplexId);
                    }

                    dbContext.Entry(complex)
                        .Collection(c => c.Members)
                        .Query().Include(m => m.User)
                        .ThenInclude(u => u.Sessions)
                        .Include(m => m.MemberAccess)
                        .Load();
                    
                    dbContext.Entry(complex).Collection(c => c.Rooms).Query()
                        .Include(r => r.Workers).Load();

                    var adminsSessionIds = (from m in complex.Members
                        where m.MemberAccess.CanModifyAccess &&
                              m.UserId != user.BaseUserId
                        from s
                            in m.User.Sessions
                        select s.SessionId).ToList();

                    var allSessionIds = (from sess in (from m in complex.Members
                            where m.UserId != user.BaseUserId
                            from s
                                in m.User.Sessions
                            select s)
                        select sess.SessionId).ToList();

                    var nonAdminSessionIds = new List<long>(allSessionIds);
                    nonAdminSessionIds.RemoveAll(sId => adminsSessionIds.Contains(sId));

                    var comp = new Complex {ComplexId = complex.ComplexId};

                    var result2 = await SharedArea.Transport
                        .RequestService<GetComplexWorkersRequest, GetComplexWorkersResponse>(
                            Program.Bus,
                            SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                            new Packet()
                            {
                                Complex = comp
                            });
                    
                    var botIds = new HashSet<long>(result2.Packet.Workerships.Select(w => w.BotId).ToList());
                    var botsQuery = dbContext.Bots.Where(b => botIds.Contains(b.BaseUserId));
                    botsQuery.Include(b => b.Sessions).Load();
                    var bots = botsQuery.ToList();
                    var botSessionIds = bots.Select(b => b.Sessions.FirstOrDefault().SessionId);
                    nonAdminSessionIds.AddRange(botSessionIds);

                    Membership lightMembership = null;
                    using (var dbContextFinal = new DatabaseContext())
                    {
                        lightMembership = dbContextFinal.Memberships.Find(membership.MembershipId);
                        dbContextFinal.Entry(lightMembership).Reference(mem => mem.User).Load();
                        dbContextFinal.Entry(lightMembership).Reference(mem => mem.Complex).Load();
                        dbContextFinal.Entry(lightMembership.Complex).Collection(c => c.Rooms).Load();
                    }

                    var mcn = new ServiceMessageNotification
                    {
                        Message = message
                    };
                    var ujnFull = new UserJointComplexNotification
                    {
                        Membership = membership
                    };
                    var ujnLight = new UserJointComplexNotification
                    {
                        Membership = lightMembership
                    };

                    SharedArea.Transport.Push<UserJointComplexPush>(
                        Program.Bus,
                        new UserJointComplexPush()
                        {
                            Notif = ujnFull,
                            SessionIds = adminsSessionIds
                        });

                    SharedArea.Transport.Push<UserJointComplexPush>(
                        Program.Bus,
                        new UserJointComplexPush()
                        {
                            Notif = ujnLight,
                            SessionIds = nonAdminSessionIds
                        });

                    SharedArea.Transport.Push<ServiceMessagePush>(
                        Program.Bus,
                        new ServiceMessagePush()
                        {
                            Notif = mcn,
                            SessionIds = allSessionIds
                        });

                    dbContext.SaveChanges();
                    dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                    dbContext.Entry(complex.ComplexSecret).Reference(cs => cs.Admin).Load();
                    var inviter = complex.ComplexSecret.Admin;
                    dbContext.Entry(inviter).Collection(i => i.Sessions).Load();

                    var inviterSessiondIds = inviter.Sessions.Select(s => s.SessionId).ToList();
                    var notification = new InviteAcceptanceNotification
                    {
                        Invite = invite
                    };

                    SharedArea.Transport.Push<InviteAcceptancePush>(
                        Program.Bus,
                        new InviteAcceptancePush()
                        {
                            Notif = notification,
                            SessionIds = inviterSessiondIds
                        });
                    
                    foreach (var room in membership.Complex.Rooms)
                    {
                        room.Workers = new List<Workership>();
                        foreach (var workership in result2.Packet.Workerships)
                            if (workership.Room.RoomId == room.RoomId)
                                room.Workers.Add(workership);
                    }

                    await context.RespondAsync(new AcceptInviteResponse()
                    {
                        Packet = new Packet {Status = "success", Membership = membership, Bots = bots, ServiceMessage = message, ComplexSecret = complex.ComplexSecret}
                    });
                }
                else
                {
                    await context.RespondAsync(new AcceptInviteResponse()
                    {
                        Packet = new Packet {Status = "error_0J0"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<IgnoreInviteRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                dbContext.Entry(human).Collection(h => h.Invites).Load();
                var invite = human.Invites.Find(i => i.ComplexId == packet.Complex.ComplexId);
                if (invite != null)
                {
                    dbContext.Entry(invite).Reference(i => i.Complex).Load();
                    var complex = invite.Complex;
                    human.Invites.Remove(invite);
                    dbContext.Invites.Remove(invite);
                    dbContext.SaveChanges();

                    SharedArea.Transport.NotifyService<InviteIgnoredNotif, InviteIgnoredNotifResponse>(
                        Program.Bus,
                        new Packet() {Invite = invite, User = human},
                        SharedArea.GlobalVariables.AllQueuesExcept(new[]
                        {
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME
                        }));

                    dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                    dbContext.Entry(complex.ComplexSecret).Reference(cs => cs.Admin).Load();
                    var inviter = complex.ComplexSecret.Admin;
                    dbContext.Entry(inviter).Collection(i => i.Sessions).Load();

                    var sessionIds = inviter.Sessions.Select(s => s.SessionId).ToList();
                    var notification = new InviteIgnoranceNotification
                    {
                        Invite = invite
                    };

                    SharedArea.Transport.Push<InviteIgnoredPush>(
                        Program.Bus,
                        new InviteIgnoredPush()
                        {
                            SessionIds = sessionIds,
                            Notif = notification
                        });

                    await context.RespondAsync(new IgnoreInviteResponse()
                    {
                        Packet = new Packet {Status = "success"}
                    });
                }
                else
                {
                    await context.RespondAsync(new IgnoreInviteResponse()
                    {
                        Packet = new Packet {Status = "error_0K0"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<GetBotsRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var bots = dbContext.Bots.Include(b => b.BotSecret).ToList();
                var finalBots = new List<Bot>();
                var bannedAccessIds = new List<long>();
                foreach (var bot in bots)
                {
                    if (bot.BotSecret.CreatorId != session.BaseUserId)
                    {
                        bannedAccessIds.Add(bot.BaseUserId);
                    }
                    else
                    {
                        finalBots.Add(bot);
                    }
                }

                using (var nextContext = new DatabaseContext())
                {
                    foreach (var id in bannedAccessIds)
                    {
                        var finalBot = nextContext.Bots.Find(id);
                        finalBots.Add(finalBot);
                    }
                }

                await context.RespondAsync(new GetBotsResponse()
                {
                    Packet = new Packet {Status = "success", Bots = finalBots}
                });
            }
        }

        public async Task Consume(ConsumeContext<GetCreatedBotsRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                var bots = new List<Bot>();
                dbContext.Entry(user).Collection(u => u.CreatedBots).Load();
                var creations = user.CreatedBots.ToList();
                foreach (var botCreation in user.CreatedBots)
                {
                    dbContext.Entry(botCreation).Reference(bc => bc.Bot).Load();
                    dbContext.Entry(botCreation.Bot).Reference(b => b.BotSecret).Load();
                    bots.Add(botCreation.Bot);
                }

                await context.RespondAsync(new GetCreatedBotsResponse()
                {
                    Packet = new Packet {Status = "success", Bots = bots, BotCreations = creations}
                });
            }
        }

        public async Task Consume(ConsumeContext<GetSubscribedBotsRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                if (session == null)
                {
                    await context.RespondAsync(new GetSubscribedBotsResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                    return;
                }

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                var bots = new List<Bot>();
                dbContext.Entry(user).Collection(u => u.SubscribedBots).Load();
                var subscriptions = user.SubscribedBots.ToList();
                var noAccessBotIds = new List<long>();
                foreach (var botSubscription in user.SubscribedBots)
                {
                    dbContext.Entry(botSubscription).Reference(bc => bc.Bot).Load();
                    dbContext.Entry(botSubscription.Bot).Reference(b => b.BotSecret).Load();
                    if (botSubscription.Bot.BotSecret.CreatorId == user.BaseUserId)
                    {
                        bots.Add(botSubscription.Bot);
                    }
                    else
                    {
                        noAccessBotIds.Add(botSubscription.Bot.BaseUserId);
                    }
                }

                using (var nextContext = new DatabaseContext())
                {
                    foreach (var id in noAccessBotIds)
                    {
                        var bot = nextContext.Bots.Find(id);
                        bots.Add(bot);
                    }
                }

                await context.RespondAsync(new GetSubscribedBotsResponse()
                {
                    Packet = new Packet {Status = "success", Bots = bots, BotSubscriptions = subscriptions}
                });
            }
        }

        public async Task Consume(ConsumeContext<SearchBotsRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;

                var bots = (from b in dbContext.Bots
                    where EF.Functions.Like(b.Title, "%" + packet.SearchQuery + "%")
                    select b).ToList();

                await context.RespondAsync(new SearchBotsResponse()
                {
                    Packet = new Packet {Status = "success", Bots = bots}
                });
            }
        }

        public async Task Consume(ConsumeContext<UpdateBotProfileRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.CreatedBots).Load();
                var botCreation = user.CreatedBots.Find(bc => bc.BotId == packet.Bot.BaseUserId);
                if (botCreation == null)
                {
                    await context.RespondAsync(new UpdateBotProfileResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                    return;
                }

                dbContext.Entry(botCreation).Reference(bc => bc.Bot).Load();
                var bot = botCreation.Bot;
                bot.Title = packet.Bot.Title;
                bot.Avatar = packet.Bot.Avatar;
                bot.Description = packet.Bot.Description;
                bot.ViewURL = packet.Bot.ViewURL;
                dbContext.SaveChanges();

                SharedArea.Transport.NotifyService<BotProfileUpdatedNotif, BotProfileUpdatedNotifResponse>(
                    Program.Bus,
                    new Packet() {Bot = bot},
                    new[]
                    {
                        SharedArea.GlobalVariables.BOT_QUEUE_NAME,
                        SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                        SharedArea.GlobalVariables.SEARCH_QUEUE_NAME,
                        SharedArea.GlobalVariables.STORE_QUEUE_NAME
                    });

                await context.RespondAsync(new UpdateBotProfileResponse()
                {
                    Packet = new Packet {Status = "success"}
                });
            }
        }

        public async Task Consume(ConsumeContext<GetBotRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var robot = (Bot) dbContext.BaseUsers.Find(packet.Bot.BaseUserId);
                if (robot == null)
                {
                    await context.RespondAsync(new GetBotResponse()
                    {
                        Packet = new Packet {Status = "error_080"}
                    });
                    return;
                }

                dbContext.Entry(robot).Reference(r => r.BotSecret).Load();
                if (robot.BotSecret.CreatorId == session.BaseUserId)
                {
                    await context.RespondAsync(new GetBotResponse()
                    {
                        Packet = new Packet {Status = "success", Bot = robot}
                    });
                    return;
                }

                using (var nextContext = new DatabaseContext())
                {
                    var nextBot = nextContext.Bots.Find(packet.Bot.BaseUserId);
                    await context.RespondAsync(new GetBotResponse()
                    {
                        Packet = new Packet {Status = "success", Bot = nextBot}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<CreateBotRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                var bot = new Bot()
                {
                    Title = packet.Bot.Title,
                    Avatar = packet.Bot.Avatar > 0 ? packet.Bot.Avatar : 0,
                    Description = packet.Bot.Description,
                    ViewURL = packet.Bot.ViewURL
                };
                var botSecret = new BotSecret()
                {
                    Bot = bot,
                    Creator = user,
                    Token = "+" + Security.MakeKey64()
                };
                bot.BotSecret = botSecret;
                var botSess = new Session()
                {
                    Token = botSecret.Token,
                    BaseUser = bot
                };
                var botCreation = new BotCreation()
                {
                    Bot = bot,
                    Creator = user
                };
                var subscription = new BotSubscription()
                {
                    Bot = bot,
                    Subscriber = user
                };
                dbContext.AddRange(bot, botSecret, botSess, botCreation, subscription);
                dbContext.SaveChanges();

                SharedArea.Transport.NotifyService<BotCreatedNotif, BotCreatedNotifResponse>(
                    Program.Bus,
                    new Packet() {Bot = bot, BotCreation = botCreation, BotSubscription = subscription, User = user},
                    new[]
                    {
                        SharedArea.GlobalVariables.BOT_QUEUE_NAME,
                        SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                        SharedArea.GlobalVariables.SEARCH_QUEUE_NAME,
                        SharedArea.GlobalVariables.STORE_QUEUE_NAME
                    });

                await context.RespondAsync(new CreateBotResponse()
                {
                    Packet = new Packet
                    {
                        Status = "success",
                        Bot = bot,
                        BotCreation = botCreation,
                        BotSubscription = subscription
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<SubscribeBotRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var bot = dbContext.Bots.Find(packet.Bot.BaseUserId);
                if (bot == null)
                {
                    await context.RespondAsync(new SubscribeBotResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.SubscribedBots).Load();
                if (user.SubscribedBots.Any(b => b.BotId == packet.Bot.BaseUserId))
                {
                    await context.RespondAsync(new SubscribeBotResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                    return;
                }

                var subscription = new BotSubscription()
                {
                    Bot = bot,
                    Subscriber = user
                };
                dbContext.AddRange(subscription);
                dbContext.SaveChanges();

                SharedArea.Transport.NotifyService<BotSubscribedNotif, BotSubscribedNotifResponse>(
                    Program.Bus,
                    new Packet() {BotSubscription = subscription, Bot = bot, User = user},
                    new[]
                    {
                        SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                        SharedArea.GlobalVariables.STORE_QUEUE_NAME,
                        SharedArea.GlobalVariables.BOT_QUEUE_NAME
                    });

                await context.RespondAsync(new SubscribeBotResponse()
                {
                    Packet = new Packet {Status = "success", BotSubscription = subscription}
                });
            }
        }

        public async Task Consume(ConsumeContext<CreateRoomRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                var mem = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (mem == null)
                {
                    await context.RespondAsync(new CreateRoomResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }

                dbContext.Entry(mem).Reference(m => m.Complex).Load();
                var complex = mem.Complex;
                if (complex != null)
                {
                    if (!string.IsNullOrEmpty(packet.Room.Title))
                    {
                        var room = new Room()
                        {
                            Title = packet.Room.Title,
                            Avatar = packet.Room.Avatar,
                            Complex = complex
                        };
                        dbContext.AddRange(room);
                        dbContext.SaveChanges();

                        SharedArea.Transport.NotifyService<RoomCreatedNotif, RoomCreatedNotifResponse>(
                            Program.Bus,
                            new Packet() {Room = room},
                            SharedArea.GlobalVariables.AllQueuesExcept(new[]
                            {
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                                SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME
                            }));

                        await SharedArea.Transport
                            .RequestService<ConsolidateCreateRoomRequest, ConsolidateCreateRoomResponse>(
                                Program.Bus,
                                SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                                new Packet() {Room = room});

                        var message = new ServiceMessage()
                        {
                            Author = null,
                            Room = room,
                            RoomId = room.RoomId,
                            Text = "Room created.",
                            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        };

                        var result = await SharedArea.Transport
                            .RequestService<PutServiceMessageRequest, PutServiceMessageResponse>(
                                Program.Bus,
                                SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                                new Packet() {ServiceMessage = message});

                        message.MessageId = result.Packet.ServiceMessage.MessageId;

                        await context.RespondAsync(new CreateRoomResponse()
                        {
                            Packet = new Packet {Status = "success", Room = room, ServiceMessage = message}
                        });
                    }
                    else
                    {
                        await context.RespondAsync(new CreateRoomResponse()
                        {
                            Packet = new Packet {Status = "error_0"}
                        });
                    }
                }
                else
                {
                    await context.RespondAsync(new CreateRoomResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<MakeAccountRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var user = context.Message.Packet.User;
                var userAuth = context.Message.Packet.UserSecret;
                var complexSecret = context.Message.Packet.ComplexSecret;

                user.UserSecret = userAuth;
                userAuth.User = user;
                userAuth.Home = user.Memberships[0].Complex;
                user.Memberships[0].Complex.ComplexSecret = complexSecret;
                complexSecret.Complex = user.Memberships[0].Complex;
                complexSecret.Admin = user;
                user.Memberships[0].Complex.Rooms[0].Complex = user.Memberships[0].Complex;
                user.Memberships[0].User = user;
                user.Memberships[0].MemberAccess.Membership = user.Memberships[0];

                dbContext.AddRange(user, userAuth, complexSecret);

                dbContext.SaveChanges();

                await context.RespondAsync(new MakeAccountResponse()
                {
                    Packet = new Packet()
                    {
                        User = user,
                        UserSecret = userAuth,
                        Complex = user.Memberships[0].Complex,
                        ComplexSecret = complexSecret,
                        Room = user.Memberships[0].Complex.Rooms[0],
                        Membership = user.Memberships[0]
                    }
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

        public async Task Consume(ConsumeContext<GetMyInvitesRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Invites).Query()
                    .Include(i => i.User).Include(i => i.Complex).Load();
                var invites = user.Invites.ToList();

                await context.RespondAsync(new GetMyInvitesResponse()
                {
                    Packet = new Packet() {Status = "success", Invites = invites}
                });
            }
        }

        public async Task Consume(ConsumeContext<UpdateMemberAccessRequest> context)
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
                    await context.RespondAsync(new UpdateMemberAccessResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }

                dbContext.Entry(membership).Reference(mem => mem.MemberAccess).Load();
                dbContext.Entry(membership).Reference(mem => mem.Complex).Load();
                var complex = membership.Complex;
                if (complex.Mode == 1 || complex.Mode == 2)
                {
                    await context.RespondAsync(new UpdateMemberAccessResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }

                dbContext.Entry(complex).Reference(c => c.ComplexSecret).Load();
                if (membership.UserId != complex.ComplexSecret.AdminId && !membership.MemberAccess.CanModifyAccess)
                {
                    await context.RespondAsync(new UpdateMemberAccessResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                    return;
                }

                dbContext.Entry(complex).Collection(c => c.Members).Load();
                var targetMem = complex.Members.Find(mem => mem.UserId == packet.User.BaseUserId);
                if (targetMem == null)
                {
                    await context.RespondAsync(new UpdateMemberAccessResponse()
                    {
                        Packet = new Packet() {Status = "error_4"}
                    });
                    return;
                }

                if (packet.User.BaseUserId == complex.ComplexSecret.AdminId ||
                    packet.User.BaseUserId == user.BaseUserId)
                {
                    await context.RespondAsync(new UpdateMemberAccessResponse()
                    {
                        Packet = new Packet() {Status = "error_5"}
                    });
                    return;
                }

                dbContext.Entry(targetMem).Reference(tmem => tmem.MemberAccess).Load();
                targetMem.MemberAccess.CanCreateMessage = packet.MemberAccess.CanCreateMessage;
                targetMem.MemberAccess.CanSendInvite = packet.MemberAccess.CanSendInvite;
                targetMem.MemberAccess.CanModifyWorkers = packet.MemberAccess.CanModifyWorkers;
                targetMem.MemberAccess.CanUpdateProfiles = packet.MemberAccess.CanUpdateProfiles;

                if (complex.ComplexSecret.AdminId == user.BaseUserId)
                    targetMem.MemberAccess.CanModifyAccess = packet.MemberAccess.CanModifyAccess;

                dbContext.SaveChanges();

                dbContext.Entry(complex.ComplexSecret).Reference(cs => cs.Admin).Load();
                dbContext.Entry(complex.ComplexSecret.Admin).Collection(u => u.Sessions).Load();

                dbContext.Entry(complex).Collection(c => c.Members).Query()
                    .Include(m => m.User)
                    .ThenInclude(u => u.Sessions)
                    .Include(m => m.MemberAccess)
                    .Load();

                var sessionIds = (from m in complex.Members
                    where
                        (m.MemberAccess.CanModifyAccess &&
                         m.User.BaseUserId != user.BaseUserId) ||
                        m.User.BaseUserId == targetMem.User.BaseUserId
                    from s in m.User.Sessions
                    select s.SessionId).ToList();

                SharedArea.Transport.Push<MemberAccessUpdatedPush>(
                    Program.Bus,
                    new MemberAccessUpdatedPush()
                    {
                        Notif = new MemberAccessUpdatedNotification()
                        {
                            MemberAccess = targetMem.MemberAccess
                        },
                        SessionIds = sessionIds
                    });

                await context.RespondAsync(new UpdateMemberAccessResponse()
                {
                    Packet = new Packet() {Status = "success"}
                });
            }
        }

        public async Task Consume(ConsumeContext<GetComplexAccessesRequest> context)
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
                    await context.RespondAsync(new GetComplexAccessesResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }
                
                dbContext.Entry(membership).Reference(mem => mem.MemberAccess).Load();
                if (!membership.MemberAccess.CanModifyAccess)
                {
                    await context.RespondAsync(new GetComplexAccessesResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }
                
                dbContext.Entry(membership).Reference(mem => mem.Complex).Load();
                var complex = membership.Complex;
                dbContext.Entry(complex).Collection(c => c.Members).Query()
                    .Include(mem => mem.MemberAccess)
                    .ThenInclude(ma => ma.Membership)
                    .Load();

                var mas = complex.Members.Select(mem => mem.MemberAccess).ToList();

                await context.RespondAsync(new GetComplexAccessesResponse()
                {
                    Packet = new Packet() {Status = "success", MemberAccesses = mas}
                });
            }
        }
    }
}