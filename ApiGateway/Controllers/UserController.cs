
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using SharedArea.Middles;
using Microsoft.AspNetCore.Mvc;
using SharedArea.Commands.User;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        [Route("~/api/user/update_user_profile")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UpdateProfile([FromBody] Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};
                
                var result = await SharedArea.Transport
                    .DirectService<UpdateUserProfileRequest, UpdateUserProfileResponse>(
                        Program.Bus,
                        SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                        session.SessionId,
                        Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        packet);

                return result.Packet;
            }
        }

        [Route("~/api/user/get_me")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetMe()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<GetMeRequest, GetMeResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()));

                return result.Packet;
            }
        }

        [Route("~/api/user/get_user_by_id")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetUserById([FromBody] Packet packet)
        {
            var result = await SharedArea.Transport.DirectService<GetUserByIdRequest, GetUserByIdResponse>(
                Program.Bus,
                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                packet);

            return result.Packet;
        }

        [Route("~/api/user/search_users")]
        [HttpPost]
        public async Task<ActionResult<Packet>> SearchUsers([FromBody] Packet packet)
        {
            var result = await SharedArea.Transport.DirectService<SearchUsersRequest, SearchUsersResponse>(
                Program.Bus,
                SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                packet);

            return result.Packet;
        }
    }
}