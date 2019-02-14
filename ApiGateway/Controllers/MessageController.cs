
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Utils;
using SharedArea.Middles;
using Microsoft.AspNetCore.Mvc;
using SharedArea.Commands.Message;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        [Route("~/api/message/get_messages")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetMessages([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0U0"};

                var result = await SharedArea.Transport.DirectService<GetMessagesRequest, GetMessagesResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/message/create_text_message")]
        [HttpPost]
        public async Task<ActionResult<Packet>> CreateTextMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<CreateTextMessageRequest, CreateTextMessageResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/message/create_file_message")]
        [HttpPost]
        public async Task<ActionResult<Packet>> CreateFileMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<CreateFileMessageRequest, CreateFileMessageResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
        
        [Route("~/api/message/bot_create_text_message")]
        [HttpPost]
        public async Task<ActionResult<Packet>> BotCreateTextMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                
                var result = await SharedArea.Transport.DirectService<BotCreateTextMessageRequest, BotCreateTextMessageResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
        
        [Route("~/api/message/bot_create_file_message")]
        [HttpPost]
        public async Task<ActionResult<Packet>> BotCreateFileMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                
                var result = await SharedArea.Transport.DirectService<BotCreateFileMessageRequest, BotCreateFileMessageResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/message/notify_message_seen")]
        [HttpPost]
        public async Task<ActionResult<Packet>> NotifyMessageSeen([FromBody] Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                
                var result = await SharedArea.Transport.DirectService<NotifyMessageSeenRequest, NotifyMessageSeenResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
        
        [Route("~/api/message/get_message_seen_count")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetMessageSeenCount([FromBody] Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                
                var result = await SharedArea.Transport.DirectService<GetMessageSeenCountRequest, GetMessageSeenCountResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
    }
}