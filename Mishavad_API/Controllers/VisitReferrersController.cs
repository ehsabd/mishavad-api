using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Mishavad_API.Models;

namespace Mishavad_API.Controllers
{
    public class VisitReferrersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/VisitReferrers
        public IQueryable<object> GetVisitReferrers()
        {
            return db.VisitReferrers
                .GroupBy(r=>r.Referrer)
                .Select(g=>new { Referrer = g.Key, Count=g.Select(x=>x.IP).Distinct().Count()})
                .OrderByDescending(o=>o.Count);
        }

        // POST: api/VisitReferrers
        [HttpPost]
        public async Task<IHttpActionResult> PostVisitReferrer(string IPAddress, string Referrer, string Url)
        {
            if (System.Web.HttpContext.Current.Request.UserHostAddress != "37.220.11.235") return Unauthorized();
            var visitReferrer = new VisitReferrer { IPAddress =  System.Net.IPAddress.Parse(IPAddress), Referrer = Referrer, Url = Url };
            db.VisitReferrers.Add(visitReferrer);
            await db.SaveChangesAsync();
            return Created("DefaultApi", visitReferrer);
        }

        // GET: api/VisitReferrers/5
        [ResponseType(typeof(VisitReferrer))]
        public async Task<IHttpActionResult> GetVisitReferrer(int id)
        {
            VisitReferrer visitReferrer = await db.VisitReferrers.FindAsync(id);
            if (visitReferrer == null)
            {
                return NotFound();
            }

            return Ok(visitReferrer);
        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool VisitReferrerExists(int id)
        {
            return db.VisitReferrers.Count(e => e.Id == id) > 0;
        }
    }
}