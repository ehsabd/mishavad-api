
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Quartz;
using Mishavad_API.Models;
namespace Mishavad_API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
           
            //Configure FileServerTokenManager
            //TODO: Migrating to a specific Config file
            Helpers.FileServerTokenManager.TokenTimeSpan = TimeSpan.FromMinutes(10);

            //TODO: Migrating to a specific Config file
            Helpers.EncryptionService.LoadBinaryFile(Server.MapPath("~/App_Data/keys.mdb"));

            var LogFilePath = Server.MapPath(string.Format("~/App_Data/ConsoleRedirect-{0}-{1}.txt", DateTime.Today.ToString("MMM"), DateTime.Today.Year));

            var consoleOut = new System.IO.StreamWriter(
                    new System.IO.FileStream(LogFilePath,
                    System.IO.FileMode.OpenOrCreate,
                    System.IO.FileAccess.Write,
                    System.IO.FileShare.ReadWrite));
            consoleOut.AutoFlush = true;
            consoleOut.BaseStream.Seek(0, System.IO.SeekOrigin.End);
            Console.SetOut(consoleOut);
            Console.WriteLine("Console set to:" + LogFilePath);
            
            Console.WriteLine(DateTime.UtcNow.ToString() + ": Application Started");
            //Uploader Size Limits
            var sizeLimits = Helpers.UploadHelper.SizeLimits;
            sizeLimits.Add(FileServerTokenType.AvatarImageUpload, 204800); // 200 kB
            sizeLimits.Add(FileServerTokenType.CampaignImageUpload, 1048576); // 1 MB
            sizeLimits.Add(FileServerTokenType.DocumentUpload, 4194304); // 4 MB
            sizeLimits.Add(FileServerTokenType.RewardImageUpload, 204800); // 200 kB

            //TODO: Remove this in-production SECURITY
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy
                = IncludeErrorDetailPolicy.Always;

            ScheduleJobs();
            
        }
        protected void Application_OnBeginRequest() {
            //TODO: you might not need this in-production this is for CORS
            var req = HttpContext.Current.Request;

            var ip = new System.Net.IPAddress(0);
            System.Net.IPAddress.TryParse(req.UserHostAddress ?? "", out ip);

            Console.WriteLine(DateTime.UtcNow.ToString() + ": " + req.HttpMethod + string.Format(" request started ({0})", ip.ToString())); 

            if (req.HttpMethod == "OPTIONS")
            {
                // ==== Respond to the OPTIONS verb =====
                var res = HttpContext.Current.Response;

                res.StatusCode = 200;
                res.End();

            } else if (req.HttpMethod=="GET")
            {
                
                var referrerUri = req.UrlReferrer;
                if (referrerUri != null && referrerUri.IsAbsoluteUri )
                {
                    var referrer = referrerUri.ToString();
                    if (referrer.Contains("localhost") || referrer.Contains("mishavad")) return;
                    //check this link for geolocation http://stackoverflow.com/questions/4327629/get-user-location-by-ip-address

                    db.VisitReferrers.Add(new VisitReferrer
                    {
                        IPAddress = ip,
                        Referrer = referrer,
                        Url = req.Url.ToString()
                    });
                    db.SaveChangesAsync();
                }
            }    
        }
        private void ScheduleJobs()
        {
            // construct a scheduler factory
            ISchedulerFactory schedFact = new Quartz.Impl.StdSchedulerFactory();

            // get a scheduler
            IScheduler sched = schedFact.GetScheduler();
            sched.Start();

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<Jobs.PreventIdleJob>()
                .WithIdentity("preventIdle", "preventIdleGroup")
                .Build();

            // Trigger the job to run now, and then every 2 mins
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("triggerPreventIdle", "preventIdleGroup")
              .StartNow()
              .WithSimpleSchedule(x => x
                  .WithIntervalInMinutes(2)
                  .RepeatForever())
              .Build();

            sched.ScheduleJob(job, trigger);
        }
    }
}
