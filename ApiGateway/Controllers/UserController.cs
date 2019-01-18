using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using MassTransit;
using SharedArea.Entities;
using SharedArea.Middles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Auth;
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
                        SharedArea.GlobalVariables.PROFILE_QUEUE_NAME,
                        session.SessionId,
                        Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        packet);

                return result.Packet;
            }
        }

        [Route("~/api/user/get_me")]
        [HttpPost]
        public ActionResult<Packet> GetMe()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Reference(u => u.UserSecret).Load();
                context.Entry(user.UserSecret).Reference(us => us.Home).Load();
                return new Packet
                {
                    Status = "success",
                    User = user,
                    UserSecret = user.UserSecret,
                    Complex = user.UserSecret.Home
                };
            }
        }

        [Route("~/api/user/get_user_by_id")]
        [HttpPost]
        public ActionResult<Packet> GetUserById([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.BaseUsers.Find(packet.BaseUser.BaseUserId);
                return new Packet {Status = "success", BaseUser = user};
            }
        }

        [Route("~/api/user/search_users")]
        [HttpPost]
        public ActionResult<Packet> SearchUsers([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var users = (from u in context.Users
                    where EF.Functions.Like(u.Title, "%" + packet.SearchQuery + "%")
                    select u).ToList();
                return new Packet {Status = "success", Users = users};
            }
        }
    }
}