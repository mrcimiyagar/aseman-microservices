
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

namespace EntryPlatform.Consumers
{
    public class EntryConsumer : IConsumer<RegisterRequest>, IConsumer<LoginRequest>, IConsumer<VerifyRequest>
        , IConsumer<LogoutRequest>, IConsumer<DeleteAccountRequest>
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
                dbContext.Entry(user).Reference(u => u.UserSecret).Load();
                dbContext.Entry(user.UserSecret).Reference(us => us.Home).Load();
                dbContext.Entry(user.UserSecret?.Home).Collection(h => h.Members).Load();
                
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
                    if (packet.VerifyCode == pending.VerifyCode)
                    {
                        User user;
                        var token = Security.MakeKey64();
                        var userAuth = dbContext.UserSecrets.FirstOrDefault(ua => ua.Email == packet.Email);
                        if (userAuth == null)
                        {
                            user = new User()
                            {
                                Title = "New User",
                                Avatar = -1
                            };                            
                            userAuth = new UserSecret()
                            {
                                User = user,
                                Email = packet.Email
                            };
                            user.UserSecret = userAuth;
                                                        
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
                            
                            var mem = new Membership()
                            {
                                User = user,
                                Complex = complex
                            };
                            
                            var result = await SharedArea.Transport.RequestService<MakeAccountRequest, MakeAccountResponse>(
                                Program.Bus,
                                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                                new Packet() {User = user, UserSecret = userAuth, Complex = complex, ComplexSecret = ca, Room = room});

                            user.BaseUserId = result.Packet.User.BaseUserId;
                            userAuth.UserSecretId = result.Packet.UserSecret.UserSecretId;
                            complex.ComplexId = result.Packet.Complex.ComplexId;
                            ca.ComplexSecretId = result.Packet.ComplexSecret.ComplexSecretId;
                            room.RoomId = result.Packet.Room.RoomId;
                            mem.MembershipId = result.Packet.Membership.MembershipId;
                            
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
                        dbContext.Entry(user.UserSecret.Home).Collection(h => h.Rooms).Load();
                        dbContext.Entry(user.UserSecret.Home).Reference(h => h.ComplexSecret).Load();
                        
                        await context.RespondAsync(
                            new VerifyResponse()
                            {
                                Packet = new Packet()
                                {
                                    Status = "success", Session = session, UserSecret = userAuth, ComplexSecret = user.UserSecret.Home.ComplexSecret
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

        public async Task Consume(ConsumeContext<DeleteAccountRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Sessions).Load();
                dbContext.Entry(user).Reference(u => u.UserSecret).Load();

                user.Title = "Deleted User";
                user.Avatar = -1;
                user.UserSecret.Email = "";                
                dbContext.Sessions.RemoveRange(user.Sessions);

                dbContext.SaveChanges();

                var allServices = SharedArea.GlobalVariables.AllQueuesExcept(new string[] {SharedArea.GlobalVariables.ENTRY_QUEUE_NAME});
                var deleteTasks = new Task[allServices.Length + 1];
                deleteTasks[0] = SharedArea.Transport.RequestService<ConsolidateDeleteAccountRequest, ConsolidateDeleteAccountResponse>(
                    Program.Bus,
                    "",
                    new Packet() {User = user, Sessions = user.Sessions});
                var counter = 1;
                foreach (var serviceName in allServices)
                {
                    deleteTasks[counter] = SharedArea.Transport.RequestService<ConsolidateDeleteAccountRequest, ConsolidateDeleteAccountResponse>(
                        Program.Bus,
                        serviceName,
                        new Packet() {User = user, Sessions = user.Sessions});
                    counter++;
                }

                Task.WaitAll(deleteTasks);

                await context.RespondAsync(new DeleteAccountResponse()
                {
                    Packet = new Packet() {Status = "success"}
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