﻿
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
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
                
                var result = await SharedArea.Transport.DirectService<CreateTextMessageRequest, CreateTextMessageResponse>(
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
                string authHeader = Request.Headers[AuthExtracter.AK];
                var parts = authHeader.Split(" ");
                var botId = long.Parse(parts[0]);
                var token = parts[1];
                var bot = context.Bots.Find(botId);
                if (bot == null) return new Packet {Status = "error_1"};
                context.Entry(bot).Reference(b => b.BotSecret).Load();
                if (bot.BotSecret.Token != token) return new Packet {Status = "error_2"};
                
                var result = await SharedArea.Transport.DirectService<BotCreateTextMessageRequest, BotCreateTextMessageResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
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
                string authHeader = Request.Headers[AuthExtracter.AK];
                var parts = authHeader.Split(" ");
                var botId = long.Parse(parts[0]);
                var token = parts[1];
                var bot = context.Bots.Find(botId);
                if (bot == null) return new Packet {Status = "error_1"};
                context.Entry(bot).Reference(b => b.BotSecret).Load();
                if (bot.BotSecret.Token != token) return new Packet {Status = "error_2"};
                
                var result = await SharedArea.Transport.DirectService<BotCreateFileMessageRequest, BotCreateFileMessageResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
    }
}