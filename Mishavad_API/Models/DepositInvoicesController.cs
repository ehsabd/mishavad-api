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

namespace Mishavad_API.Models
{
    [Authorize]
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

        // POST: api/DepositInvoices/5/FromBank
        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IHttpActionResult> FromBankDepositInvoice(long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!DepositInvoiceExists(id))
            {
                return NotFound();
            }

            /*
            

                Codes to validate payment and do transactions etc
            
            
            */

          //  db.Entry(depositInvoice).State = EntityState.Modified;

            
                await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/DepositInvoices
        [ResponseType(typeof(object))]
        [HttpPost]
        public async Task<IHttpActionResult> PostDepositInvoice(DepositInvoice depositInvoice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           
            if (DepositInvoiceExists(depositInvoice.Id))
            {
                db.Entry(depositInvoice).State = EntityState.Modified;
            }
            else {
                db.DepositInvoices.Add(depositInvoice);
            }
            // add code to create signature if needed (Pasargad):
            //Move these to a method to free space here
            // var sign = RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            /*  rsa.FromXmlString(“< RSAKeyValue >< Modulus > oQRshGhLf2Fh...”);
              کلید خصوصی فروشنده//
  string data = "#" + merchantCode + "#" + terminalCode + "#"
  + invoiceNumber + "#" + invoiceDate + "#" + amount + "#" + redirectAddress
  + "#" + action + "#" + timeStamp + "#";
              byte[] signMain = rsa.SignData(Encoding.UTF8.GetBytes(data), new
              SHA1CryptoServiceProvider());
              sign = Convert.ToBase64String(signMain);*/

              await db.SaveChangesAsync();
            var sign = "";

            return CreatedAtRoute("DefaultApi", new { id = depositInvoice.Id }, new
            {
                Id = depositInvoice.Id,
                Sign = sign
            });
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
    }
}