using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotPlatform.DbContexts;
using BotPlatform.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedArea.Commands.Bot;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Pushes;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using SharedArea.Utils;

namespace BotPlatform.Consumers
{
    public class BotConsumer : IConsumer<BotSubscribedNotif>, IConsumer<BotCreatedNotif>
    {
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
    }
}