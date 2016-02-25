using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Mishavad_API.Models;

using System.Threading.Tasks;

namespace Mishavad_API.Controllers
{
    [Authorize(Roles = "admin")]
    public class IncomingFundOrdersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/IncomingFundOrders
        public IQueryable<IncomingFundOrder> GetIncomingFundOrders()
        {
            return db.IncomingFundOrders;
        }

        // GET: api/IncomingFundOrders/5
        [ResponseType(typeof(IncomingFundOrder))]
        public IHttpActionResult GetIncomingFundOrder(long id)
        {
            IncomingFundOrder incomingFundOrder = db.IncomingFundOrders.Find(id);
            if (incomingFundOrder == null)
            {
                return NotFound();
            }

            return Ok(incomingFundOrder);
        }

        // POST (Update): api/IncomingFundOrders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PostIncomingFundOrder(long id, IncomingFundOrder incomingFundOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != incomingFundOrder.Id)
            {
                return BadRequest();
            }

            db.Entry(incomingFundOrder).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IncomingFundOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        /*
        // POST (Update): api/IncomingFundOrders/Approve/id
        [Route("api/IncomingFundOrders/Approve")]
        [ResponseType(typeof(void))]
        [HttpPost]
        [AllowAnonymous]
        
        iN : InvoiceNumber (Invoice.Id)
        iD : InvoiceDate   (Invoice.CreatedDateUtc)
        tref: TransactionReferenceId*/

        /*    public IHttpActionResult ApproveIncomingFundOrder([FromUri] string iN, [FromUri] string iD, [FromUri] string tref)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                /*TODO: Add The code to check that transaction has been done with Bank for TransactionReferenceID here:

                */
        /*NOTE: We don't have to check invoice number because there are no chance of human error in the process
        and we check TransactionReferenceID (tref) with bank

        var order_id = long.Parse(Helpers.AccountNumberHelper.RemoveSalt(iN));
        var order = db.IncomingFundOrders.Find(order_id);

        if (order==null)
             return NotFound();

        order.Status = Mishavad_API.Models.OrderStatus.BankApproved;

        if (order.GiftFundCampaignId != null)
        {
            var giftFund = new GiftFund
            {
                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,
                Amount = order.Amount,
                CampaignId = (int)order.GiftFundCampaignId
            };

            db.GiftFunds.Add(giftFund);
        }

        db.SaveChanges();

        return StatusCode(HttpStatusCode.NoContent);
    }
    }
*//*
        // POST: api/IncomingFundOrders
        [ResponseType(typeof(IncomingFundOrder))]
        [AllowAnonymous]
        public IHttpActionResult PostIncomingFundOrder(IncomingFundOrderVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var incomingFundOrder = new IncomingFundOrder
            {
                InvNumSalt = Helpers.AccountNumberHelper.GenerateNumericSalt(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Amount = model.Amount,
                Status = Mishavad_API.Models.OrderStatus.BankInProcess,
                GiftFundCampaignId = model.GiftFundCampaignId
              };

            db.IncomingFundOrders.Add(incomingFundOrder);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = incomingFundOrder.Id }, incomingFundOrder);
        }
        */
    /*
        // DELETE: api/IncomingFundOrders/5
        [ResponseType(typeof(IncomingFundOrder))]
        public IHttpActionResult DeleteIncomingFundOrder(int id)
        {
            IncomingFundOrder incomingFundOrder = db.IncomingFundOrders.Find(id);
            if (incomingFundOrder == null)
            {
                return NotFound();
            }

            db.IncomingFundOrders.Remove(incomingFundOrder);
            db.SaveChanges();

            return Ok(incomingFundOrder);
        }*/

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IncomingFundOrderExists(long id)
        {
            return db.IncomingFundOrders.Count(e => e.Id == id) > 0;
        }
    }
}