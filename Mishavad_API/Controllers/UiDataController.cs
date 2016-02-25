using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Mishavad_API.Models;
using System.Web.Http.Description;

namespace Mishavad_API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public class UiDataController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: api/UiData/EditCampaign
        [Route("api/UiData/EditCampaign")]
        public object GetEditCampaignUiData()
        {
            return new { CampaignCategories = db.CampaignCategories,
                     ProjectStages = db.ProjectStages,
                     Cities = db.Cities};
        }
    }
}
