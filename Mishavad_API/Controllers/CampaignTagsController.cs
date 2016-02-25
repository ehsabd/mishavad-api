using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Mishavad_API.Models;
namespace Mishavad_API.Controllers
{
    public class CampaignTagsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/CampaignTags/abc
        [Route("CampaignTags/{text}")]
        public IEnumerable<string> Get(string text)
        {
            return db.CampaignTagMaps.Where(m=>m.CampaignTagName.Contains(text)).Select(m=>m.CampaignTagName).Take(10);
        }

    }
}
