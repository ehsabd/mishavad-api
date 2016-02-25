using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Mishavad_API.Models;

namespace Mishavad_API
{
    
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
        }
        
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new CustomUserStore(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            //TODO: For future we can save this in PublicSettings and use it to do synchronized client-side validation
            //TODO: Length required should be transfered to either Public or Private Setting.
            //ehsabd: We used a custom password validator to display custom validation message
            CustomizePasswordValidation.LengthRequired = 8; //static variable
            manager.PasswordValidator = new CustomizePasswordValidation();

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = System.TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;



            /*
            // Register two factor authentication providers. This application uses Phone 
            // and Emails as a step of receiving a code for verifying the user 
            // You can write your own provider and plug in here. 
            manager.RegisterTwoFactorProvider("PhoneCode",
                new PhoneNumberTokenProvider<ApplicationUser, int>
                {
                    MessageFormat = "Your security code is: {0}"
                });
            manager.RegisterTwoFactorProvider("EmailCode",
                new EmailTokenProvider<ApplicationUser, int>
                {
                    Subject = "Security Code",
                    BodyFormat = "Your security code is: {0}"
                });
            manager.SmsService = new SmsService();
            */

            // Configure Mail Service
            manager.EmailService = new EmailService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, int>(dataProtectionProvider.Create("Identity of Mishavad 30912640"));
            }
            

            return manager;
        }

    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return configOutlookasync(message);
        }

        private Task configOutlookasync(IdentityMessage message)
        {
            // Credentials:
            var credentialUserName = "ehsabd@outlook.com";
            var sentFrom = "ehsabd@outlook.com";
            var pwd = System.Configuration.ConfigurationManager.AppSettings["mailPass"];

            // Configure the client:
            System.Net.Mail.SmtpClient client =
                new System.Net.Mail.SmtpClient("smtp-mail.outlook.com");

            client.Port = 587;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            // Create the credentials:
            System.Net.NetworkCredential credentials =
                new System.Net.NetworkCredential(credentialUserName, pwd);

            client.EnableSsl = true;
            client.Credentials = credentials;

            // Create the message:
            var mail =
                new System.Net.Mail.MailMessage(sentFrom, message.Destination);
            mail.From = new System.Net.Mail.MailAddress("noreply@mishavad.ir");
            mail.Subject = message.Subject;
            mail.Body =  message.Body;//TODO This should be plain text .Escape <a> tag from html message body

            // pass message body as an alternate body to the mail message.
            // the Encoding should be set to prevent question marks in different Mail Apps!
            var alternate = 
                System.Net.Mail.AlternateView.CreateAlternateViewFromString(
                    message.Body,System.Text.Encoding.UTF8,"text/html");
            mail.AlternateViews.Add(alternate);

            // Send:
            return client.SendMailAsync(mail);
        }
    }

}
