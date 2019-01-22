using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Hubs;
using ApiGateway.Utils;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedArea.Commands.Room;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public RoomController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
        [Route("~/api/room/create_room")]
        [HttpPost]
        public async Task<ActionResult<Packet>> CreateRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };
                
                var result = await SharedArea.Transport.DirectService<CreateRoomRequest, CreateRoomResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
        
        [Route("~/api/room/update_room_profile")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UpdateRoomProfile([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };

                var result = await SharedArea.Transport.DirectService<UpdateRoomProfileRequest, UpdateRoomProfileResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/room/delete_room")]
        [HttpPost]
        public async Task<ActionResult<Packet>> DeleteRoom([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };

                var result = await SharedArea.Transport.DirectService<DeleteRoomRequest, DeleteRoomResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/room/get_rooms")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetRooms([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0B1"};
                
                var result = await SharedArea.Transport.DirectService<GetRoomsRequest, GetRoomsResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.SEARCH_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/room/get_room_by_id")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetRoomById([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<GetRoomByIdRequest, GetRoomByIdResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.SEARCH_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }
    }
}