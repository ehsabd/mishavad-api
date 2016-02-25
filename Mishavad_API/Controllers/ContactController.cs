using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Mishavad_API.Models;
using System.Threading.Tasks;
namespace Mishavad_API.Controllers
{
    [RoutePrefix("api/Contact")]
    public class ContactController : ApiController
    {
        // GET: api/Contact
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Contact/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Contact
        [Route("")]
        public async Task<IHttpActionResult> Post(ContactBM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailService = new EmailService();
            try
            {
                await emailService.SendAsync(new Microsoft.AspNet.Identity.IdentityMessage
                {
                    Body = string.Format("<p>Pandool Contact Me Form:</p><p>Name:{0}</p><p>Email:{1}</p><p>{2}</p>", model.Name, model.Email, model.Message),
                    Subject = "Pandool contact me from " + model.Name,
                    Destination = "azizi.m1390@gmail.com"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Contact Me Email Error:" + ex.Message);
            }
            return Ok();

        }

    }
}
