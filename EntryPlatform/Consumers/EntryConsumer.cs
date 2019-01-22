
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using EntryPlatform.DbContexts;
using EntryPlatform.Utils;
using MassTransit;
using SharedArea.Commands.Auth;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Utils;

namespace EntryPlatform.Consumers
{
    public class EntryConsumer : IConsumer<RegisterRequest>, IConsumer<LoginRequest>, IConsumer<VerifyRequest>
        , IConsumer<LogoutRequest>
    {
        private const string EmailAddress = "keyhan.mohammadi1997@gmail.com";
        private const string EmailPassword = "2&b165sf4j)684tkt87El^o9w68i87u6s*4h48#98aq";
        
        public async Task Consume(ConsumeContext<RegisterRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var code = Security.MakeKey8();
                SendEmail(packet.Email,
                    "Verify new device",
                    "Your verification code is : " + code);
                var pending = dbContext.Pendings.FirstOrDefault(p => p.Email == packet.Email);
                if (pending == null)
                {
                    pending = new Pending
                    {
                        Email = packet.Email,
                        VerifyCode = code
                    };
                    dbContext.AddRange(pending);
                    dbContext.SaveChanges();
                }
                else
                {
                    pending.VerifyCode = code;
                }
                dbContext.SaveChanges();
                await context.RespondAsync(
                    new RegisterResponse()
                    {
                        Packet = new Packet()
                        {
                            Status = "success"
                        }
                    });
            }
        }

        public async Task Consume(ConsumeContext<LoginRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                session.Token = Security.MakeKey64();
                dbContext.SaveChanges();
                dbContext.Entry(user).Reference(u => u.UserSecret).Load();
                dbContext.Entry(user.UserSecret).Reference(us => us.Home).Load();
                dbContext.Entry(user.UserSecret?.Home).Collection(h => h.Members).Load();
                
                SharedArea.Transport.NotifyService<SessionUpdatedNotif>(
                    Program.Bus,
                    new Packet() {Session = session},
                    SharedArea.GlobalVariables.AllQueuesExcept(new []
                    {
                        SharedArea.GlobalVariables.ENTRY_QUEUE_NAME
                    }));
                
                await context.RespondAsync(
                    new LoginResponse()
                    {
                        Packet = new Packet()
                        {
                            Status = "success", Session = session
                        }
                    });
            }
        }

        public async Task Consume(ConsumeContext<VerifyRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var pending = dbContext.Pendings.FirstOrDefault(p => p.Email == packet.Email);
                if (pending != null)
                {
                    if (packet.VerifyCode == "123")
                    {
                        User user;
                        var token = Security.MakeKey64();
                        var userAuth = dbContext.UserSecrets.FirstOrDefault(ua => ua.Email == packet.Email);
                        if (userAuth == null)
                        {
                            user = new User()
                            {
                                Title = "",
                                Avatar = -1
                            };                            
                            userAuth = new UserSecret()
                            {
                                User = user,
                                Email = packet.Email
                            };
                            user.UserSecret = userAuth;
                            
                            var result = await SharedArea.Transport.RequestService<PutUserRequest, PutUserResponse>(
                                Program.Bus,
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                                new Packet() {User = user, UserSecret = userAuth});
                            
                            user.BaseUserId = result.Packet.User.BaseUserId;
                            userAuth.UserSecretId = result.Packet.UserSecret.UserSecretId;
                            
                            var complex = new Complex()
                            {
                                Title = "Home",
                                Avatar = 0,
                                Mode = 1
                            };
                            var ca = new ComplexSecret()
                            {
                                Admin = user,
                                Complex = complex
                            };
                            
                            complex.ComplexSecret = ca;
                            var room = new Room()
                            {
                                Title = "Main",
                                Avatar = 0,
                                Complex = complex
                            };
                            
                            userAuth.Home = complex;
                            
                            var result2 = await SharedArea.Transport.RequestService<PutComplexRequest, PutComplexResponse>(
                                Program.Bus,
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                                new Packet() {User = user, Complex = complex, ComplexSecret = ca, Room = room});
                            
                            complex.ComplexId = result2.Packet.Complex.ComplexId;
                            ca.ComplexSecretId = result2.Packet.ComplexSecret.ComplexSecretId;
                            room.RoomId = result2.Packet.Room.RoomId;

                            await SharedArea.Transport.RequestService<UpdateUserSecretRequest, UpdateUserSecretResponse>(
                                Program.Bus,
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                                new Packet() {UserSecret = userAuth});
                            
                            var mem = new Membership()
                            {
                                User = user,
                                Complex = complex
                            };
                            
                            var result3 = await SharedArea.Transport.RequestService<PutMembershipRequest, PutMembershipResponse>(
                                Program.Bus,
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                                new Packet() {Membership = mem, User = user, Complex = complex});
                            
                            mem.MembershipId = result3.Packet.Membership.MembershipId;
                            
                            dbContext.AddRange(user, userAuth, complex, ca, room, mem);

                            dbContext.SaveChanges();
                            
                            SharedArea.Transport.NotifyService<UserCreatedNotif>(
                                Program.Bus,
                                new Packet() {User = user, UserSecret = userAuth},
                                SharedArea.GlobalVariables.AllQueuesExcept(new []
                                {
                                    SharedArea.GlobalVariables.ENTRY_QUEUE_NAME,
                                    SharedArea.GlobalVariables.CITY_QUEUE_NAME
                                }));
                            
                            SharedArea.Transport.NotifyService<ComplexCreatedNotif>(
                                Program.Bus,
                                new Packet() {User = user, Complex = complex, ComplexSecret = ca, Room = room, Membership = mem},
                                SharedArea.GlobalVariables.AllQueuesExcept(new []
                                {
                                    SharedArea.GlobalVariables.ENTRY_QUEUE_NAME,
                                    SharedArea.GlobalVariables.CITY_QUEUE_NAME
                                }));
                        }
                        else
                        {
                            dbContext.Entry(userAuth).Reference(us => us.User).Load();
                            user = userAuth.User;
                        }
                        var session = new Session()
                        {
                            Token = token,
                            ConnectionId = "",
                            Online = false,
                            BaseUser = user
                        };

                        var result4 = await SharedArea.Transport.RequestService<PutSessionRequest, PutSessionResponse>(
                            Program.Bus,
                            SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                            new Packet() {Session = session, BaseUser = user});

                        session.SessionId = result4.Packet.Session.SessionId;
                        
                        dbContext.AddRange(session);
                        dbContext.Pendings.Remove(pending);
                        dbContext.SaveChanges();
                        
                        SharedArea.Transport.NotifyService<SessionCreatedNotif>(
                            Program.Bus,
                            new Packet() {Session = session, BaseUser = user},
                            SharedArea.GlobalVariables.AllQueuesExcept(new []
                            {
                                SharedArea.GlobalVariables.ENTRY_QUEUE_NAME,
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME
                            }));
                        
                        dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                        dbContext.Entry(user).Reference(u => u.UserSecret).Load();
                        dbContext.Entry(user.UserSecret).Reference(us => us.Home).Load();
                        dbContext.Entry(user.UserSecret.Home).Collection(h => h.Members).Load();
                        dbContext.Entry(user.UserSecret.Home).Reference(h => h.ComplexSecret).Load();
                        
                        await context.RespondAsync(
                            new VerifyResponse()
                            {
                                Packet = new Packet()
                                {
                                    Status = "success", Session = session, UserSecret = userAuth
                                }
                            });
                    }
                    else
                    {
                        await context.RespondAsync(
                            new VerifyResponse()
                            {
                                Packet = new Packet()
                                {
                                    Status = "error_020"
                                }
                            });
                    }
                }
                else
                {
                    await context.RespondAsync(
                        new VerifyResponse()
                        {
                            Packet = new Packet()
                            {
                                Status = "error_021"
                            }
                        });
                }
            }
        }
        
        public async Task Consume(ConsumeContext<LogoutRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                session.ConnectionId = "";
                session.Online = false;
                dbContext.SaveChanges();
                await context.RespondAsync(
                    new LogoutResponse()
                    {
                        Packet = new Packet()
                        {
                            Status = "success"
                        }
                    });
            }
        }

        private static void SendEmail(string to, string subject, string content)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(EmailAddress, EmailPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(EmailAddress)
            };
            mailMessage.To.Add(to);
            mailMessage.Body = content;
            mailMessage.Subject = subject;
            client.Send(mailMessage);
        }
    }
}