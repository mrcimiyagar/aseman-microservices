
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Hubs;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Complex;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ComplexController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public ComplexController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }

        [Route("~/api/complex/update_complex_profile")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UpdateComplexProfile([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_1" };

                var result = await SharedArea.Transport
                    .DirectService<UpdateComplexProfileRequest, UpdateComplexProfileResponse>(
                        Program.Bus,
                        SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                        session.SessionId,
                        Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        packet);

                return result.Packet;
            }
        }

        [Route("~/api/complex/create_complex")]
        [HttpPost]
        public async Task<ActionResult<Packet>> CreateComplex([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_1" };

                var result = await SharedArea.Transport.DirectService<CreateComplexRequest, CreateComplexResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/complex/delete_complex")]
        [HttpPost]
        public async Task<ActionResult<Packet>> DeleteComplex([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet { Status = "error_2" };
                
                var result = await SharedArea.Transport.DirectService<DeleteComplexRequest, DeleteComplexResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/complex/get_complexes")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetComplexes()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<GetComplexesRequest, GetComplexesResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()));

                return result.Packet;
            }
        }

        [Route("~/api/complex/get_complex_by_id")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetComplexById([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<GetComplexByIdRequest, GetComplexByIdResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/complex/search_complexes")]
        [HttpPost]
        public async Task<ActionResult<Packet>> SearchComplexes([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                var result = await SharedArea.Transport.DirectService<SearchComplexesRequest, SearchComplexesResponse>(
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