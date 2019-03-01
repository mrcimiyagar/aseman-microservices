using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Hubs;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using SharedArea.Entities;
using SharedArea.Notifications;
using SharedArea.Utils;
using File = System.IO.File;
using JsonSerializer = SharedArea.Utils.JsonSerializer;

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
                        var findable = mongo.GetNotifsColl().Find(notif => notif.Session.SessionId == sessionId);
                        var n = findable.FirstOrDefault();
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

        private async void PushNotification(Session session, Notification notif)
        {
            notif.Type = notif.GetType().Name;
            
            Logger.Log("NotificationsLog", $"Pushing notification : {Environment.NewLine + JsonSerializer.SerializeObject(notif) + Environment.NewLine} to client...");
            
            if (notif.GetType() == typeof(AudioMessageNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyAudioMessageReceived", notif);
            }
            else if (notif.GetType() == typeof(BotAdditionToRoomNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotAddedToRoom", notif);
            }
            else if (notif.GetType() == typeof(BotAnimatedBotViewNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotAnimatedBotView", notif);
            }
            else if (notif.GetType() == typeof(BotRanCommandsOnBotViewNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotRanCommandsOnBotView", notif);
            }
            else if (notif.GetType() == typeof(BotRemovationFromRoomNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotRemovedFromRoom", notif);
            }
            else if (notif.GetType() == typeof(BotSentBotViewNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotSentBotView", notif);
            }
            else if (notif.GetType() == typeof(BotUpdatedBotViewNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyBotUpdatedBotView", notif);
            }
            else if (notif.GetType() == typeof(ComplexDeletionNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyComplexDeleted", notif);
            }
            else if (notif.GetType() == typeof(ContactCreationNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyContactCreated", notif);
            }
            else if (notif.GetType() == typeof(InviteAcceptanceNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteAccepted", notif);
            }
            else if (notif.GetType() == typeof(InviteCancellationNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteCancelled", notif);
            }
            else if (notif.GetType() == typeof(InviteCreationNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteCreated", notif);
            }
            else if (notif.GetType() == typeof(InviteIgnoranceNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyInviteIgnored", notif);
            }
            else if (notif.GetType() == typeof(PhotoMessageNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyPhotoMessageReceived", notif);
            }
            else if (notif.GetType() == typeof(RoomDeletionNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyRoomDeleted", notif);
            }
            else if (notif.GetType() == typeof(ServiceMessageNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyServiceMessageReceived", notif);
            }
            else if (notif.GetType() == typeof(TextMessageNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyTextMessageReceived", notif);
            }
            else if (notif.GetType() == typeof(UserJointComplexNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyUserJointComplex", notif);
            }
            else if (notif.GetType() == typeof(UserRequestedBotViewNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyUserRequestedBotView", notif);
            }
            else if (notif.GetType() == typeof(VideoMessageNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyVideoMessageReceived", notif);
            }
            else if (notif.GetType() == typeof(MessageSeenNotification))
            {
                await _notifsHub.Clients.Client(session.ConnectionId)
                    .SendAsync("NotifyMessageSeen", notif);
            }
        }
    }
}