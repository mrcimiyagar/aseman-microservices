
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MessengerPlatform.DbContexts;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Entities;
using SharedArea.Utils;

namespace MessengerPlatform.Consumers
{
    public class NotifConsumer : GlobalConsumer<DatabaseContext>, IConsumer<BotProfileUpdatedNotif>, IConsumer<WorkershipCreatedNotif>
        , IConsumer<WorkershipUpdatedNotif>, IConsumer<WorkershipDeletedNotif>, IConsumer<VideoCreatedNotif>, IConsumer<PhotoCreatedNotif>
        , IConsumer<AudioCreatedNotif>, IConsumer<BotCreatedNotif>
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
        
        public async Task Consume(ConsumeContext<WorkershipCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var workership = context.Message.Packet.Workership;
                workership.Room = dbContext.Rooms.Find(workership.Room.RoomId);

                dbContext.Workerships.Add(workership);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new WorkershipCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipUpdatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalWs = context.Message.Packet.Workership;

                var localWs = dbContext.Workerships.Find(globalWs.WorkershipId);

                localWs.PosX = globalWs.PosX;
                localWs.PosY = globalWs.PosY;
                localWs.Width = globalWs.Width;
                localWs.Height = globalWs.Height;

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new WorkershipUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipDeletedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalWs = context.Message.Packet.Workership;

                var localWs = dbContext.Workerships.Find(globalWs.WorkershipId);
                dbContext.Entry(localWs).Reference(ws => ws.Room).Load();
                var room = localWs.Room;

                room.Workers.Remove(localWs);
                dbContext.Workerships.Remove(localWs);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new WorkershipDeletedNotifResponse());
        }

        public async Task Consume(ConsumeContext<BotProfileUpdatedNotif> context)
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

            await context.RespondAsync(new BotProfileUpdatedNotifResponse());
        }
        
        public async Task Consume(ConsumeContext<PhotoCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var file = context.Message.Packet.Photo;
                var fileUsage = context.Message.Packet.FileUsage;
                var uploader = dbContext.BaseUsers.Find(context.Message.Packet.BaseUser.BaseUserId);

                if (fileUsage != null)
                {
                    file.Uploader = uploader;
                    file.FileUsages.Clear();
                    fileUsage.File = file;
                    fileUsage.Room = dbContext.Rooms.Find(fileUsage.Room.RoomId);
                    dbContext.AddRange(fileUsage);
                }
                else
                    dbContext.AddRange(file);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new PhotoCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<AudioCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var file = context.Message.Packet.Audio;
                var fileUsage = context.Message.Packet.FileUsage;
                var uploader = dbContext.BaseUsers.Find(context.Message.Packet.BaseUser.BaseUserId);

                if (fileUsage != null)
                {
                    file.Uploader = uploader;
                    file.FileUsages.Clear();
                    fileUsage.File = file;
                    fileUsage.Room = dbContext.Rooms.Find(fileUsage.Room.RoomId);
                    dbContext.AddRange(fileUsage);
                }
                else
                    dbContext.AddRange(file);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new AudioCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<VideoCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var file = context.Message.Packet.Video;
                var fileUsage = context.Message.Packet.FileUsage;
                var uploader = dbContext.BaseUsers.Find(context.Message.Packet.BaseUser.BaseUserId);

                if (fileUsage != null)
                {
                    file.Uploader = uploader;
                    file.FileUsages.Clear();
                    fileUsage.File = file;
                    fileUsage.Room = dbContext.Rooms.Find(fileUsage.Room.RoomId);
                    dbContext.AddRange(fileUsage);
                }
                else
                    dbContext.AddRange(file);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new VideoCreatedNotifResponse());
        }
    }
}