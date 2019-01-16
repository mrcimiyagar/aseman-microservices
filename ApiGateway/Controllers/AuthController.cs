
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiGateway;
using MessengerPlatform.DbContexts;
using SharedArea.Entities;
using SharedArea.Middles;
using AWP.Utils;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Auth;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {

        [Route("~/api/auth/register")]
        [HttpPost]
        public async Task<ActionResult<Packet>> Register([FromBody] Packet packet)
        {
            var address = new Uri(SharedArea.GlobalVariables.API_GATEWAY_INTERNAL_QUEUE_PATH);
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);

            IRequestClient<RegisterRequest, RegisterResponse> client =
                new MessageRequestClient<RegisterRequest, RegisterResponse>(Program.Bus, address, requestTimeout);

            var result = await client.Request(new RegisterRequest()
            {
                Packet = packet
            });

            return result.Packet;
        }

        [Route("~/api/auth/login")]
        [HttpPost]
        public async Task<ActionResult<Packet>> Login()
        {
            var address = new Uri(SharedArea.GlobalVariables.API_GATEWAY_INTERNAL_QUEUE_PATH);
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);

            IRequestClient<LoginRequest, LoginResponse> client =
                new MessageRequestClient<LoginRequest, LoginResponse>(Program.Bus, address, requestTimeout);

            var result = await client.Request(new LoginRequest()
            {
                Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString())
            });

            return result.Packet;
        }

        [Route("~/api/auth/verify")]
        [HttpPost]
        public ActionResult<Packet> Verify([FromBody] Packet packet)
        {
            
        }

        [Route("~/api/auth/logout")]
        [HttpPost]
        public ActionResult<Packet> Logout()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_040" };
                session.ConnectionId = "";
                session.Online = false;
                context.SaveChanges();
                return new Packet { Status = "success" };
            }
        }
    }
}