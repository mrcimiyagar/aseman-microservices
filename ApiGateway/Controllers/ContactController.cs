using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Hubs;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedArea.Commands.Contact;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public ContactController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
        [Route("~/api/contact/create_contact")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetContacts([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_051"};

                var result = await SharedArea.Transport.DirectService<CreateContactRequest, CreateContactResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.CITY_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/contact/get_contacts")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetContacts()
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_060"};

                var result = await SharedArea.Transport.DirectService<GetContactsRequest, GetContactsResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.SEARCH_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()));

                return result.Packet;
            }
        }
    }
}