using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Bot;
using StorePlatform.DbContexts;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Entities;
using SharedArea.Middles;

namespace StorePlatform.Consumers
{
    public class StoreConsumer : IConsumer<GetBotStoreContentRequest>, IConsumer<ConsolidateDeleteAccountRequest>
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