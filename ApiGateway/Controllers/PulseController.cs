using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Utils;
using Microsoft.AspNetCore.Mvc;
using SharedArea.Commands.Pulse;
using SharedArea.Middles;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class PulseController : Controller
    {
        [Route("~/api/pulse/request_bot_view")]
        public async Task<ActionResult<Packet>> RequestBotView(Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<RequestBotViewRequest, RequestBotViewResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.BOT_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/pulse/bot_send_bot_view")]
        public async Task<ActionResult<Packet>> BotSendBotView(Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<SendBotViewRequest, SendBotViewResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.BOT_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/pulse/bot_update_bot_view")]
        public async Task<ActionResult<Packet>> BotUpdateBotView(Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<UpdateBotViewRequest, UpdateBotViewResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.BOT_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/pulse/bot_animate_bot_view")]
        public async Task<ActionResult<Packet>> BotAnimateBotView(Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<AnimateBotViewRequest, AnimateBotViewResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.BOT_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
        
        [Route("~/api/pulse/bot_run_commands_on_bot_view")]
        public async Task<ActionResult<Packet>> BotRunCommandsOnBotView(Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<RunCommandsOnBotViewRequest, RunCommandsOnBotViewResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.BOT_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
    }
}