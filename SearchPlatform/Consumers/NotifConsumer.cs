
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using SearchPlatform.DbContexts;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Entities;
using SharedArea.Utils;

namespace SearchPlatform.Consumers
{
    public class NotifConsumer : GlobalConsumer<DatabaseContext>, IConsumer<BotCreatedNotif>
        , IConsumer<BotProfileUpdatedNotif>
    {
        public async Task Consume(ConsumeContext<BotCreatedNotif> context)
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

            await context.RespondAsync(new BotCreatedNotifResponse());
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