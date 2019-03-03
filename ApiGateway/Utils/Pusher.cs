using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Hubs;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SharedArea.Entities;
using SharedArea.Notifications;

namespace ApiGateway.Utils
{
    public class Pusher
    {
        enum NotifSenderState { WaitingForAck, EmptyQueue }
        
        private readonly IHubContext<NotificationsHub> _notifsHub;
        private readonly Dictionary<long, CancellationTokenSource> _cancellationTokens;
        private readonly Dictionary<long, NotifSenderState> _senderStates;
        private readonly Dictionary<long, object> _stateLocks;
        
        public Pusher(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
            _cancellationTokens = new Dictionary<long, CancellationTokenSource>();
            _senderStates = new Dictionary<long, NotifSenderState>();
            _stateLocks = new Dictionary<long, object>();
        }

        public void NotifyNotificationReceived(long sessionId)
        {
            _senderStates[sessionId] = NotifSenderState.EmptyQueue;
        }

        public void NextPush(long sessionId)
        {
            if (_senderStates.ContainsKey(sessionId))
            {
                lock (_stateLocks[sessionId])
                {
                    if (_senderStates[sessionId] == NotifSenderState.WaitingForAck) return;
                }
            }
            else
            {
                _stateLocks[sessionId] = new object();
            }
            
            _senderStates[sessionId] = NotifSenderState.WaitingForAck;
            
            try
            {
                _cancellationTokens[sessionId]?.Cancel();
            }
            catch (Exception)
            {
                // ignored
            }
            
            var cts = new CancellationTokenSource();
            _cancellationTokens[sessionId] = cts;

            Task.Run(async () =>
            {
                using (var dbContext = new DatabaseContext())
                {
                    var session = dbContext.Sessions.Find(sessionId);
                    if (!session.Online)
                    {
                        _senderStates[sessionId] = NotifSenderState.EmptyQueue;
                        return;
                    }
                    using (var mongo = new MongoLayer())
                    {
                        var coll = mongo.GetNotifsColl();
                        
                        var findFluent = await coll.FindAsync(notif => notif["Session"]["SessionId"] == sessionId, cancellationToken: cts.Token);
                        var n = await findFluent.FirstOrDefaultAsync(cts.Token);
                        if (n == null)
                        {
                            _senderStates[sessionId] = NotifSenderState.EmptyQueue;
                            return;
                        }
                        PushNotification(session, n);
                        await Task.Delay(10000, cts.Token);
                        _senderStates[sessionId] = NotifSenderState.EmptyQueue;
                        NextPush(sessionId);
                    }
                }
            }, cts.Token);
        }

        private async void PushNotification(Session session, BsonDocument n)
        {
            var type = n["_t"].ToString();
            
            if (type == typeof(AudioMessageNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<AudioMessageNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyAudioMessageReceived", notif);
            }
            else if (type == typeof(BotAdditionToRoomNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<BotAdditionToRoomNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotAddedToRoom", notif);
            }
            else if (type == typeof(BotAnimatedBotViewNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<BotAnimatedBotViewNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotAnimatedBotView", notif);
            }
            else if (type == typeof(BotRanCommandsOnBotViewNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<BotRanCommandsOnBotViewNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotRanCommandsOnBotView", notif);
            }
            else if (type == typeof(BotRemovationFromRoomNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<BotRemovationFromRoomNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotRemovedFromRoom", notif);
            }
            else if (type == typeof(BotSentBotViewNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<BotSentBotViewNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotSentBotView", notif);
            }
            else if (type == typeof(BotUpdatedBotViewNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<BotUpdatedBotViewNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotUpdatedBotView", notif);
            }
            else if (type == typeof(ComplexDeletionNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<ComplexDeletionNotification>(n);
                notif.Type = notif.GetType().Name;
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyComplexDeleted", notif);
            }
            else if (type == typeof(ContactCreationNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<ContactCreationNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyContactCreated", notif);
            }
            else if (type == typeof(InviteAcceptanceNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<InviteAcceptanceNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteAccepted", notif);
            }
            else if (type == typeof(InviteCancellationNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<InviteCancellationNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteCancelled", notif);
            }
            else if (type == typeof(InviteCreationNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<InviteCreationNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteCreated", notif);
            }
            else if (type == typeof(InviteIgnoranceNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<InviteIgnoranceNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteIgnored", notif);
            }
            else if (type == typeof(PhotoMessageNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<PhotoMessageNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyPhotoMessageReceived", notif);
            }
            else if (type == typeof(RoomDeletionNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<RoomDeletionNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyRoomDeleted", notif);
            }
            else if (type == typeof(ServiceMessageNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<ServiceMessageNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyServiceMessageReceived", notif);
            }
            else if (type == typeof(TextMessageNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<TextMessageNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyTextMessageReceived", notif);
            }
            else if (type == typeof(UserJointComplexNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<UserJointComplexNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyUserJointComplex", notif);
            }
            else if (type == typeof(UserRequestedBotViewNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<UserRequestedBotViewNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyUserRequestedBotView", notif);
            }
            else if (type == typeof(VideoMessageNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<VideoMessageNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyVideoMessageReceived", notif);
            }
            else if (type == typeof(MessageSeenNotification).Name)
            {
                var notif = BsonSerializer.Deserialize<MessageSeenNotification>(n);
                notif.Type = notif.GetType().Name;
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyMessageSeen", notif);
            }
        }
    }
}