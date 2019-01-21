using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotPlatform.DbContexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Bot;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Pushes;
using SharedArea.Consumers;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using SharedArea.Utils;

namespace BotPlatform.Consumers
{
    public class BotConsumer : NotifConsumer, IConsumer<GetBotsRequest>, IConsumer<GetCreatedBotsRequest>
        , IConsumer<GetSubscribedBotsRequest>, IConsumer<SearchBotsRequest>, IConsumer<UpdateBotProfileRequest>
        , IConsumer<GetBotRequest>, IConsumer<SubscribeBotRequest>, IConsumer<CreateBotRequest>
    {
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
                bot.ViewURL = packet.Bot.ViewURL;
                dbContext.SaveChanges();
                
                SharedArea.Transport.NotifyService<BotProfileUpdatedNotif>(
                    Program.Bus,
                    new Packet() {Bot = bot},
                    new [] {SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME});
                
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
                var robot = dbContext.Bots.Find(packet.Bot.BaseUserId);
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
                
                SharedArea.Transport.NotifyService<BotSubscribedNotif>(
                    Program.Bus,
                    new Packet() {BotSubscription = subscription, Bot = bot, User = user},
                    new []
                    {
                        SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                        SharedArea.GlobalVariables.STORE_QUEUE_NAME
                    });
                
                await context.RespondAsync(new SubscribeBotResponse()
                {
                    Packet = new Packet {Status = "success", BotSubscription = subscription}
                });
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
                    ViewURL = packet.Bot.ViewURL
                };
                var botSecret = new BotSecret()
                {
                    Bot = bot,
                    Creator = user,
                    Token = Security.MakeKey64()
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
                
                SharedArea.Transport.NotifyService<BotCreatedNotif>(
                    Program.Bus,
                    new Packet() {Bot = bot, BotCreation = botCreation, BotSubscription = subscription, User = user},
                    new []
                    {
                        SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                        SharedArea.GlobalVariables.STORE_QUEUE_NAME,
                        SharedArea.GlobalVariables.BOT_QUEUE_NAME
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
    }
}