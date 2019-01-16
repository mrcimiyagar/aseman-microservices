using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using EntryPlatform.DbContexts;
using EntryPlatform.Utils;
using MassTransit;
using SharedArea.Commands;
using SharedArea.Commands.Auth;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Entities;
using SharedArea.Middles;

namespace EntryPlatform.Consumers
{
    public class EntryConsumer : IConsumer<RegisterRequest>, IConsumer<LoginRequest>, IConsumer<VerifyRequest>
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
                await context.RespondAsync<RegisterResponse>(
                    new RegisterResponse()
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

        public async Task Consume(ConsumeContext<LoginRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = Security.Authenticate(dbContext, context.Message.Headers[AuthExtracter.AK]);
                if (session == null)
                {
                    await context.RespondAsync<RegisterResponse>(
                        new RegisterResponse()
                        {
                            Packet = new Packet()
                            {
                                Status = "error_030"
                            }
                        });
                    return;
                }
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                session.Token = Security.MakeKey64();
                dbContext.SaveChanges();
                dbContext.Entry(user).Reference(u => u.UserSecret).Load();
                dbContext.Entry(user.UserSecret).Reference(us => us.Home).Load();
                dbContext.Entry(user.UserSecret?.Home).Collection(h => h.Members).Load();
                await context.RespondAsync<RegisterResponse>(
                    new RegisterResponse()
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
                            
                            var address = new Uri(SharedArea.GlobalVariables.CITY_QUEUE_PATH);
                            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);            
                            IRequestClient<NotifyUserCreatedRequest, NotifyUserCreatedResponse> client =
                                new MessageRequestClient<NotifyUserCreatedRequest, NotifyUserCreatedResponse>
                                    (Program.Bus, address, requestTimeout);
                            var result = await client.Request(new NotifyUserCreatedRequest()
                            {
                                Packet = new Packet()
                                {
                                    User = user,
                                    UserSecret = userAuth
                                }
                            });
                            
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
                            
                            address = new Uri(SharedArea.GlobalVariables.CITY_QUEUE_PATH);
                            requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);            
                            IRequestClient<NotifyComplexCreatedRequest, NotifyComplexCreatedResponse> client2 =
                                new MessageRequestClient<NotifyComplexCreatedRequest, NotifyComplexCreatedResponse>
                                    (Program.Bus, address, requestTimeout);
                            result = await client.Request(new NotifyComplexCreatedRequest()
                            {
                                Packet = new Packet()
                                {
                                    Complex = complex,
                                    ComplexSecret = ca
                                }
                            });
                            
                            var mem = new Membership()
                            {
                                User = user,
                                Complex = complex
                            };
                            dbContext.AddRange(user, userAuth, complex, ca, room, mem);
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
                        dbContext.AddRange(session);
                        dbContext.Pendings.Remove(pending);
                        dbContext.SaveChanges();

                        dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                        dbContext.Entry(user).Reference(u => u.UserSecret).Load();
                        dbContext.Entry(user.UserSecret).Reference(us => us.Home).Load();
                        dbContext.Entry(user.UserSecret.Home).Collection(h => h.Members).Load();
                        dbContext.Entry(user.UserSecret.Home).Reference(h => h.ComplexSecret).Load();

                        return new Packet { Status = "success", Session = session, UserSecret = userAuth };
                    }
                    else
                    {
                        return new Packet { Status = "error_020" };
                    }
                }
                else
                {
                    return new Packet { Status = "error_021" };
                }
            }
        }
    }
}