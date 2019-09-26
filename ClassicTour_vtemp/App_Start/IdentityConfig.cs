using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using ClassicTour_vtemp.Models;
using System.Net.Mail;
using System.Net;
using SmsIrRestful;

namespace ClassicTour_vtemp
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var token = new Token().GetToken("14efab49bf025f903ebafb4", "LanaDelRey");

            var ultraFastSend = new UltraFastSend()
            {
                Mobile = long.Parse(message.Destination)
            };

            switch (message.Subject)
            {
                case Smstemplate.Signup:
                    ultraFastSend.TemplateId = int.Parse(Smstemplate.Signup);
                    string[] up = message.Body.Split('%');
                    ultraFastSend.ParameterArray = new List<UltraFastParameters>() {
                        new UltraFastParameters() { Parameter = "username", ParameterValue = up[0] },
                        new UltraFastParameters() { Parameter = "password", ParameterValue = up[1] }
                    }.ToArray();
                    break;
                case Smstemplate.Payment_Done:
                    ultraFastSend.TemplateId = int.Parse(Smstemplate.Payment_Done);
                    ultraFastSend.ParameterArray = new List<UltraFastParameters>() {
                        new UltraFastParameters() { Parameter = "price", ParameterValue = message.Body }
                    }.ToArray();
                    break;
                case Smstemplate.Cerdit_Payment:
                    ultraFastSend.TemplateId = int.Parse(Smstemplate.Payment_Done);
                    string[] down = message.Body.Split('%');
                    ultraFastSend.ParameterArray = new List<UltraFastParameters>() {
                        new UltraFastParameters() { Parameter = "price", ParameterValue = down[0] },
                        new UltraFastParameters() { Parameter = "code", ParameterValue = down[1] }
                    }.ToArray();
                    break;
            }

            UltraFastSendRespone ultraFastSendRespone = new UltraFast().Send(token, ultraFastSend);
            if (ultraFastSendRespone.IsSuccessful)
            {
                return Task.FromResult(1);
            }
            else
            {
                return Task.FromResult(0);
            }
        }
    }

    public static class Smstemplate
    {
        public const string Signup = "5202";
        public const string Payment_Done = "5603";
        public const string Cerdit_Payment = "5681";
    }

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = false
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = false;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
