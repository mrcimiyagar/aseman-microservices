using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedArea.Commands.Bot;
using StorePlatform.DbContexts;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Entities;
using SharedArea.Middles;

namespace StorePlatform.Consumers
{
    public class StoreConsumer : IConsumer<GetBotStoreContentRequest>, IConsumer<BotSubscribedNotif>
        , IConsumer<BotCreatedNotif>, IConsumer<BotProfileUpdatedNotif>
    {
        public async Task Consume(ConsumeContext<GetBotStoreContentRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var botStoreHeader = dbContext.BotStoreHeader
                    .Include(bsh => bsh.Banners)
                    .ThenInclude(b => b.Bot)
                    .FirstOrDefault();
                var botStoreSection = new BotStoreSection();
                var botStoreBots = dbContext.Bots.Select(bot => new BotStoreBot()
                    {
                        Bot = bot,
                        BotStoreSection = botStoreSection
                    })
                    .ToList();
                botStoreSection.BotStoreBots = botStoreBots;

                await context.RespondAsync(new GetBotStoreContentResponse()
                {
                    Packet = new Packet
                    {
                        Status = "success",
                        BotStoreHeader = botStoreHeader,
                        BotStoreSections = new List<BotStoreSection>() { botStoreSection }
                    }
                });
            }
        }

        public Task Consume(ConsumeContext<BotSubscribedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var subscription = context.Message.Packet.BotSubscription;
                var globalBot = context.Message.Packet.Bot;
                var globalUser = context.Message.Packet.User;

                var localUser = dbContext.Users.Find(globalUser.BaseUserId);
                var localBot = dbContext.Bots.Find(globalBot.BaseUserId);

                subscription.Bot = localBot;
                subscription.Subscriber = localUser;

                dbContext.BotSubscriptions.Add(subscription);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalUser = context.Message.Packet.User;
                var localUser = (User) dbContext.BaseUsers.Find(globalUser.BaseUserId);
                var bot = context.Message.Packet.Bot;
                var botSecret = bot.BotSecret;
                var session = bot.Sessions.FirstOrDefault();
                var creation = context.Message.Packet.BotCreation;
                var subscription = context.Message.Packet.BotSubscription;

                creation.Bot = bot;
                creation.Creator = localUser;

                subscription.Bot = bot;
                subscription.Subscriber = localUser;

                dbContext.AddRange(bot, botSecret, session, creation, subscription);

                dbContext.SaveChanges();
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotProfileUpdatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalBot = context.Message.Packet.Bot;

                var localBot = dbContext.Bots.Find(globalBot.BaseUserId);

                localBot.Title = globalBot.Title;
                localBot.Avatar = globalBot.Avatar;
                localBot.Description = globalBot.Description;
                localBot.ViewURL = globalBot.ViewURL;

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }
    }
}