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
    [RoutePrefix("api/DepositInvoices")]
    public class DepositInvoicesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

       

        // GET: api/DepositInvoices
        public IQueryable<DepositInvoice> GetDepositInvoices()
        {
            return db.DepositInvoices;
        }

        // GET: api/DepositInvoices/5
        [ResponseType(typeof(DepositInvoice))]
        public async Task<IHttpActionResult> GetDepositInvoice(long id)
        {
            DepositInvoice depositInvoice = await db.DepositInvoices.FindAsync(id);
            if (depositInvoice == null)
            {
                return NotFound();
            }

            return Ok(depositInvoice);
        }

       
        [ResponseType(typeof(void))]
        [HttpPost]
        [Route("{id?}/zarinpal/paid", Name ="VerifyZarinpal")]
        public async Task<IHttpActionResult> ZarinpalPaid(long id, string Authority, string Status)
        {
            if (string.IsNullOrEmpty(Authority) || string.IsNullOrEmpty(Status))
            {
                return BadRequest();
            }

            if (Status != "OK")
            {
                return InternalServerError(new Exception("Zarinpal gateway error, Authority:"+Authority+", Status:" + Status));
            }

            var invoice = await db.DepositInvoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            System.Net.ServicePointManager.Expect100Continue = false;
            Zarinpal.PaymentGatewayImplementationServicePortTypeClient zp = new Zarinpal.PaymentGatewayImplementationServicePortTypeClient();
            
            long RefID;
            int verStatus = zp.PaymentVerification("YOUR-ZARINPAL-MERCHANT-CODE",Authority, invoice.Amount, out RefID);

            if (verStatus == 100)
            {
                invoice.ReferenceNumber = RefID.ToString();
                //1) Create an account for the payer according to AccountName
                //2) Create two journal entries to debit the campaign and credit the user
                //3) Create the related transaction

            }
            else {
                return InternalServerError(new Exception("Zarinpal gateway error, status:" + verStatus.ToString()));
            }
            db.Entry(invoice).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("new/zarinpal/",Name = "NewZarinpalDepositInvoice")]
        // POST: api/DepositInvoices
        [HttpPost]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> NewZarinpalDepositInvoice(NewDepositInvoiceBM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Zarinpal.PaymentGatewayImplementationServicePortTypeClient zp = 
                new Zarinpal.PaymentGatewayImplementationServicePortTypeClient();
            
            var invoice = new DepositInvoice
            {
                Amount = model.Amount,
                PaymentGateway = PaymentGateway.ZarinPal,
                ReceiverCampaignId = model.ReceiverCampaignId,
                AccountName = model.Email,
                ExtraInfoJSON= new ExtraInfoJSON { InfoJSON=model.ExtraInfoJSON}
            };
            db.DepositInvoices.Add(invoice);
            await db.SaveChangesAsync();
            //NOTE: We do not provide Zarinpal with our customers' email or mobile number
            //NOTE that we used Async version and changed the original source code, we should check whether this works
             var resp = await zp.PaymentRequestAsync("YOUR-ZARINPAL-MERCHANT-CODE", model.Amount,model.Description,
                "", "",  Url.Link("VerifyZarinpal",new { id=invoice.Id})
                );
            var status = resp.Body.Status;
            var Authority = resp.Body.Authority;
            if (status == 100)
            {
                return Created("DefaultApi", new { Authority = Authority });
            }
            else {
                return InternalServerError(new Exception("Zarinpal gateway error, status:" + status.ToString()));
            }       
        }


        [Route("new/fake/", Name = "NewFakeDepositInvoice")]
        // POST: api/DepositInvoices
        [HttpPost]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> NewFakeDepositInvoice(NewDepositInvoiceBM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Zarinpal.PaymentGatewayImplementationServicePortTypeClient zp =
                new Zarinpal.PaymentGatewayImplementationServicePortTypeClient();

            var invoice = new DepositInvoice
            {
                Amount = model.Amount,
                PaymentGateway = PaymentGateway.Fake,
                ReceiverCampaignId = model.ReceiverCampaignId,
                AccountName = model.Email,
                ExtraInfoJSON = new ExtraInfoJSON { InfoJSON = model.ExtraInfoJSON }
            };
            db.DepositInvoices.Add(invoice);
            await db.SaveChangesAsync();
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DepositInvoiceExists(long id)
        {
            return db.DepositInvoices.Count(e => e.Id == id) > 0;
        }
        /*
        private async Task AddRelatedTransaction()
        {

        }*/
    }
}