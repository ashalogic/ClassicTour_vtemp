using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ClassicTour_vtemp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Orders = new List<Order>();
            Credit_Orders = new List<Credit_Order>();
        }

        [Required]
        public string first_name { get; set; }
        [Required]
        public string last_name { get; set; }
        [Required]
        public string national_code { get; set; }
        [Required]
        public Gender gender { get; set; }

        public List<Order> Orders { get; set; }

        public List<Credit_Order> Credit_Orders { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    [Table("Tours")]
    public class Tour
    {
        [Key]
        public string id { get; set; }
        [Required]
        public string title { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int tourcode { get; set; }

        [Required]
        public string startdate { get; set; }
        [Required]
        public string enddate { get; set; }
        [Required]
        public int days { get; set; }
        [Required]
        public int nights { get; set; }
        [Required]
        public int capacity { get; set; }
        [Required]
        public string startpoint { get; set; }//مکان شروع حرکت
        [Required]
        public string city { get; set; }//شهر محل برگزاری گشت
        [Required]
        public string price { get; set; }
        //[Required]
        public string img { get; set; }
        public string alt { get; set; }
        [Required]
        public bool status { get; set; }
        [Required]
        public string description { get; set; }
    }

    [Table("Orders")]
    public class Order
    {
        [Key]
        public string id { get; set; }

        [Required]
        public string price { get; set; }

        [Required]
        public int count { get; set; }

        [Required]
        public virtual Tour tour { get; set; }

        [Required]
        public virtual ApplicationUser customer { get; set; }

        [Required]
        public System.DateTime date { get; set; }

        [Required]
        public PaymentMethod paymentmethod { get; set; }

        //Online Prop
        public string authority { get; set; }
        public string refid { get; set; }

        //C2C Prop
        public string trackid { get; set; }

        //C2C,Credit Ppurchase track
        public string sellerid { get; set; }

        [Required]
        public VerificationStatus verificationstatus { get; set; }

        public bool status { get; set; }
    }

    [Table("Credit_Orders")]
    public class Credit_Order
    {
        [Key]
        public string id { get; set; }

        [Required]
        public string price { get; set; }

        [Required]
        public virtual ApplicationUser customer { get; set; }

        [Required]
        public string providerid { get; set; }

        [Required]
        public System.DateTime date { get; set; }

        public string description { get; set; }

        public bool verified { get; set; }

        public bool status { get; set; }

        public bool Paid { get; set; }
    }

    public enum PaymentMethod
    {
        Online, Cart2Cart, Cerdit
    }
    public enum VerificationStatus
    {
        verified, notverified
    }
    public enum Gender
    {
        male, female
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("CTourDBCon", throwIfV1Schema: false)
        {
        }

        public DbSet<Credit_Order> Credit_Orders { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Tour> Tours { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}