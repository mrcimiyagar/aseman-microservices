
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Utils;
using SharedArea.Middles;
using Microsoft.AspNetCore.Mvc;
using SharedArea.Commands.Bot;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : Controller
    {
        [Route("~/api/robot/get_bots")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetBots()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<GetBotsRequest, GetBotsResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()));

                return result.Packet;
            }
        }

        [Route("~/api/robot/add_bot_to_room")]
        [HttpPost]
        public async Task<ActionResult<Packet>> AddBotToRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_3"};
                
                var result = await SharedArea.Transport.DirectService<AddBotToRoomRequest, AddBotToRoomResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/robot/get_bot_store_content")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetBotStore()
        {
            var result = await SharedArea.Transport.DirectService<GetBotStoreContentRequest, GetBotStoreContentResponse>(
                Program.Bus,
                SharedArea.GlobalVariables.STORE_QUEUE_NAME,
                Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()));

            return result.Packet;
        }

        [Route("~/api/robot/update_workership")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UpdateWorkership([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                
                var result = await SharedArea.Transport.DirectService<UpdateWorkershipRequest, UpdateWorkershipResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/robot/get_created_bots")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetCreatedBots()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<GetCreatedBotsRequest, GetCreatedBotsResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()));

                return result.Packet;
            }
        }

        [Route("~/api/robot/get_subscribed_bots")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetSubscribedBots()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<GetSubscribedBotsRequest, GetSubscribedBotsResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()));

                return result.Packet;
            }
        }

        [Route("~/api/robot/subscribe_bot")]
        [HttpPost]
        public async Task<ActionResult<Packet>> SubscribeBot([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_2"};
                
                var result = await SharedArea.Transport.DirectService<SubscribeBotRequest, SubscribeBotResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/robot/create_bot")]
        [HttpPost]
        public async Task<ActionResult<Packet>> CreateBot([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<CreateBotRequest, CreateBotResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/robot/get_robot")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetRobot([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_081"};
                
                var result = await SharedArea.Transport.DirectService<GetBotRequest, GetBotResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/robot/update_bot_profile")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UpdateBotProfile([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_1"};
                
                var result = await SharedArea.Transport.DirectService<UpdateBotProfileRequest, UpdateBotProfileResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/robot/get_workerships")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetWorkerships([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<GetWorkershipsRequest, GetWorkershipsResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/robot/search_bots")]
        [HttpPost]
        public async Task<ActionResult<Packet>> SearchBots([FromBody] Packet packet)
        {
            var result = await SharedArea.Transport.DirectService<SearchBotsRequest, SearchBotsResponse>(
                Program.Bus,
                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                packet);

            return result.Packet;
        }

        [Route("~/api/robot/remove_bot_from_room")]
        [HttpPost]
        public async Task<ActionResult<Packet>> RemoveBotFromRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_2"};
                
                var result = await SharedArea.Transport.DirectService<RemoveBotFromRoomRequest, RemoveBotFromRoomResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.DESKTOP_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
    }
}