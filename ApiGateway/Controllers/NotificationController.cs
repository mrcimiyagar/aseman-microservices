using System.Collections.Generic;
using System.Linq;
using MessengerPlatform.DbContexts;
using SharedArea.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController
    {
        public ActionResult<List<Notification>> Get()
        {
            using (var context = new DatabaseContext())
            {
                return context.Notifications.ToList();
            }
        }
    }
}