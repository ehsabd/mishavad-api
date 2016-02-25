using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Net;
namespace Mishavad_API.Jobs
{
    public class PreventIdleJob : IJob
    {
        public void Execute(IJobExecutionContext context) {
            WebClient client = new WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            var s = client.DownloadString("http://mishavad.ir/api/Campaigns");

            
        }
    }
}