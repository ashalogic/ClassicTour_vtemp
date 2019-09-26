using ClassicTour_vtemp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClassicTour_vtemp.Startup))]
namespace ClassicTour_vtemp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //CreateRoles();
            //CreateMaster();
        }

        private void CreateRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!RoleManager.RoleExists("Customer"))
            {
                var role = new IdentityRole()
                {
                    Name = "Customer"
                };
                RoleManager.Create(role);
            }

            if (!RoleManager.RoleExists("Admin"))
            {
                var role = new IdentityRole()
                {
                    Name = "Admin"
                };
                RoleManager.Create(role);
            }

            if (!RoleManager.RoleExists("Seller"))
            {
                var role = new IdentityRole()
                {
                    Name = "Seller"
                };
                RoleManager.Create(role);
            }
            if (!RoleManager.RoleExists("Provider"))
            {
                var role = new IdentityRole()
                {
                    Name = "Provider"
                };
                RoleManager.Create(role);
            }

        }

        private void CreateMaster()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var _Master = new ApplicationUser()
            {
                Email = "rafee1410@yahoo.com",
                first_name = "منوچهر",
                last_name = "رفیعی",
                national_code = "2431827739",
                PhoneNumber = "09177055900",
                UserName = "09177055900",
                PhoneNumberConfirmed = true,
                LockoutEnabled = false,
                EmailConfirmed = true,
                gender = Gender.male
            };

            var result = UserManager.Create(_Master, "2345900");
            if (result.Succeeded)
            {
                UserManager.AddToRole(_Master.Id, "Admin");
            }
        }
    }
}
