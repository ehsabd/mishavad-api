using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Mishavad_API.Helpers
{
    public class CustomHttpExceptions
    {
        public static void CustomBadRequest(string reason)
        {
            CustomHttpException(HttpStatusCode.BadRequest, reason);
        }
        public static void CustomHttpException(HttpStatusCode status, string reason)
        {
            var reasonPhrase = status.ToString() + " - " + reason;
            Console.WriteLine("HTTP EXCEPTION:" + reasonPhrase);
            throw new HttpResponseException(new HttpResponseMessage(status)
            { ReasonPhrase = reasonPhrase });
        }
        
    }
}