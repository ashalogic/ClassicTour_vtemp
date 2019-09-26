using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ClassicTour_vtemp.Models;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Data;

namespace ClassicTour_vtemp.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //_______________________________________________________________________________________________________________| .:: توابع مسیریابی ::.

        public ActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Admin");
            if (User.IsInRole("Seller"))
                return RedirectToAction("Seller");
            if (User.IsInRole("Customer"))
                return RedirectToAction("Customer");
            if (User.IsInRole("Provider"))
                return RedirectToAction("Provider");
            return RedirectToAction("index", "home");
        }

        [Authorize(Roles = "Customer")]
        public ActionResult Customer()
        {
            var uid = User.Identity.GetUserId();
            var user = UserManager.Users.Where(x => x.Id == uid).FirstOrDefault();
            if (user != null)
                ViewBag.theme = user.gender;
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Admin()
        {
            var uid = User.Identity.GetUserId();
            var user = UserManager.Users.Where(x => x.Id == uid).FirstOrDefault();
            if (user != null)
                ViewBag.theme = user.gender;
            return View();
        }

        [Authorize(Roles = "Seller")]
        public ActionResult Seller()
        {
            var uid = User.Identity.GetUserId();
            var user = UserManager.Users.Where(x => x.Id == uid).FirstOrDefault();
            if (user != null)
                ViewBag.theme = user.gender;
            return View();
        }

        [Authorize(Roles = "Provider")]
        public ActionResult Provider()
        {
            var uid = User.Identity.GetUserId();
            var user = UserManager.Users.Where(x => x.Id == uid).FirstOrDefault();
            if (user != null)
                ViewBag.theme = user.gender;
            return View();
        }

        //_______________________________________________________________________________________________________________| .:: توابع مشتریان ::.

        [Authorize(Roles = "Admin")]
        [HttpGet]
        /// <summary>
        ///تابع ایجاد مدیر حدید
        /// </summary>
        /// <param name="first_name">نام</param>
        /// <param name="last_name"> نام خانوادکی</param>
        /// <param name="gender">جنسیت</param>
        /// <param name="phone_number">شماره موبایل</param>
        /// <param name="national_code">کد ملی</param>
        /// <param name="pass_word">گذرواژه</param>
        /// <returns></returns>
        public async Task<string> Add_Customer(string first_name, string last_name, int gender, string phone_number, string national_code, string pass_word)
        {
            if (!string.IsNullOrWhiteSpace(first_name) && !string.IsNullOrWhiteSpace(last_name) && !string.IsNullOrWhiteSpace(phone_number) && !string.IsNullOrWhiteSpace(national_code) && !string.IsNullOrWhiteSpace(pass_word))
            {
                try
                {
                    var customer = new ApplicationUser()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = "nomail@nomail.com",
                        PhoneNumberConfirmed = true,

                        first_name = first_name.ToString(),
                        last_name = last_name.ToString(),
                        gender = (Gender)gender,
                        PhoneNumber = phone_number.ToString(),
                        national_code = national_code.ToString(),
                        UserName = phone_number.ToString()
                    };
                    var res = await UserManager.CreateAsync(customer, pass_word);
                    if (res.Succeeded)
                    {
                        var res2 = await UserManager.AddToRoleAsync(customer.Id, "Customer");
                        if (res2.Succeeded)
                        {
                            await new SmsService().SendAsync(new IdentityMessage()
                            {
                                Body = customer.UserName + "%" + pass_word,
                                Destination = phone_number,
                                Subject = Smstemplate.Signup
                            });
                            return string.Format("مشتری مورد نظر با شماره موبایل {0} افزوده شد", customer.PhoneNumber);
                        }
                        else
                        {
                            return string.Format("مشتری مورد نظر با شماره موبایل {0} افزوده شد اما نقش مشتری الحاق نشد", customer.PhoneNumber);
                        }
                    }
                    else
                    {
                        return "خطا در افزودن مشتری";
                    }
                }
                catch
                {
                    return "خطا در افزودن مشتری";
                }
            }
            else
            {
                return "خطا در افزودن مشتری";
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller,Provider")]
        public JsonResult Get_Customers()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var users = db.Users.ToList();
            List<ApplicationUser> res = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (UserManager.IsInRole(user.Id, "Customer"))
                {
                    res.Add(user);
                }
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<string> Reset_Customer_Password(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                string ResToken = await UserManager.GeneratePasswordResetTokenAsync(id);
                string PassWord = System.Web.Security.Membership.GeneratePassword(8, 0);
                var result = await UserManager.ResetPasswordAsync(id, ResToken, PassWord);
                if (result.Succeeded)
                {
                    await new SmsService().SendAsync(new IdentityMessage()
                    {
                        Body = user.UserName + "%" + PassWord,
                        Destination = user.PhoneNumber,
                        Subject = Smstemplate.Signup
                    });
                    return "OK";
                }
                else
                {
                    return "NOK";
                }
            }
            return "NOK";
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> Add_Customer_Range(List<ClipboardCustomerModel> customers)
        {
            List<string> Err = new List<string>();

            foreach (ClipboardCustomerModel customer in customers)
            {
                if ((customer.phone).ToCharArray()[0] != '0')
                {
                    customer.phone = "0" + customer.phone;
                }

                ApplicationUser user = new ApplicationUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "nomail@nomail.com",
                    PhoneNumberConfirmed = true,

                    first_name = customer.fname,
                    last_name = customer.lname,
                    national_code = customer.ncode,
                    PhoneNumber = customer.phone,
                    UserName = customer.phone
                };

                if (customer.gender == "زن")
                    user.gender = Gender.female;
                else
                    user.gender = Gender.male;

                string pass_word = System.Web.Security.Membership.GeneratePassword(8, 0);
                var res = await UserManager.CreateAsync(user, pass_word);
                if (res.Succeeded)
                {
                    var res2 = await UserManager.AddToRoleAsync(user.Id, "Customer");
                    if (res2.Succeeded)
                    {
                        await new SmsService().SendAsync(new IdentityMessage()
                        {
                            Body = user.UserName + "%" + pass_word,
                            Destination = user.PhoneNumber,
                            Subject = Smstemplate.Signup
                        });
                    }
                    else
                        Err.Add(customer.ncode + ";" + res2.Errors.ToList()[0]);
                }
                else
                    Err.Add(customer.ncode + ";" + res.Errors.ToList()[0]);
            }

            if (Err.Count > 0)
                return Json(Err, JsonRequestBehavior.AllowGet);
            else
                return Json(new string[] { "با موفیت انجام شد" }, JsonRequestBehavior.AllowGet);
        }

        //_______________________________________________________________________________________________________________| .:: توابع تور ها ::.

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<string> Add_Tour(string title, string startdate, string enddate, int days, int nights, int capacity, string startpoint, string city, string price, string img, string alt, string description)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var tour = new Tour()
            {
                id = Guid.NewGuid().ToString(),
                status = true,

                title = title,
                alt = alt,
                capacity = capacity,
                startpoint = startpoint,
                startdate = startdate,
                enddate = enddate,
                img = img,
                days = days,
                city = city,
                description = description,
                nights = nights,
                price = price
            };
            db.Tours.Add(tour);
            var res = await db.SaveChangesAsync();
            if (res == 3 || res == 1)
            {
                return string.Format("تور مورد نظر با نام {0} افزوده شد", tour.title);
            }
            else
            {
                return "خطا در افزودن تور";
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<string> Toggle_Tour_Status(string id)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var res = db.Tours.Where(x => x.id == id.Trim()).FirstOrDefault();
            if (res != null)
            {
                res.status = !res.status;
                await db.SaveChangesAsync();
                return "OK";
            }

            return "NOK";
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public string Upload_Img()
        {
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];

                var validImageTypes = new string[]
            {
                    "image/gif",
                    "image/jpeg",
                    "image/pjpeg",
                    "image/png"
            };

                if (file != null && file.ContentLength > 0 && validImageTypes.Contains(file.ContentType))
                {
                    var uploadDir = "/Content/img/tours/";
                    var imagePath = Path.Combine(uploadDir, DateTime.Now.ToFileTimeUtc().ToString() + "." + file.FileName.Split('.').LastOrDefault());
                    file.SaveAs(Server.MapPath(imagePath));
                    return imagePath;
                }
                else
                {
                    return "خطا";
                }
            }
            return "خطا";
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public JsonResult Get_Tours()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var tours = db.Tours.ToList();
            return Json(tours, JsonRequestBehavior.AllowGet);
        }

        //_______________________________________________________________________________________________________________| .:: توابع سفارشات ها ::.

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<string> Add_Order(string tourid, string userid, string count, string trackid = "")
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var tour = db.Tours.Where(x => x.id == tourid).FirstOrDefault();
            var user = db.Users.Where(x => x.Id == userid).FirstOrDefault();
            var sellerid = User.Identity.GetUserId();

            if (tour != null && tour.capacity >= int.Parse(count) && user != null && !string.IsNullOrWhiteSpace(sellerid))
            {
                var order = new Order()
                {
                    id = Guid.NewGuid().ToString(),

                    sellerid = sellerid,
                    customer = user,
                    tour = tour,

                    count = int.Parse(count),
                    price = (int.Parse(tour.price) * int.Parse(count)).ToString(),

                    date = DateTime.Now.ToUniversalTime(),
                };

                if (!string.IsNullOrWhiteSpace(trackid) && trackid != "undefined")
                {
                    order.verificationstatus = VerificationStatus.verified;
                    order.paymentmethod = PaymentMethod.Cart2Cart;
                    order.trackid = trackid;
                    order.status = true;
                    new SmsService().Send(new IdentityMessage() { Subject = Smstemplate.Payment_Done, Body = order.price, Destination = User.Identity.GetUserName() });
                }
                else
                {
                    order.verificationstatus = VerificationStatus.notverified;
                    order.paymentmethod = PaymentMethod.Cerdit;
                    order.status = false;
                    new SmsService().Send(new IdentityMessage() { Subject = Smstemplate.Cerdit_Payment, Body = order.price + "%" + new Random().Next(int.MinValue, int.MaxValue), Destination = User.Identity.GetUserName() });
                }

                user.Orders.Add(order);

                await new SmsService().SendAsync(new IdentityMessage()
                {
                    Destination = user.PhoneNumber,
                    Subject = Smstemplate.Signup
                });



                tour.capacity -= int.Parse(count);
                if (tour.capacity <= 0) tour.status = false;

                await db.SaveChangesAsync();
                return string.Format("خرید تور {0} برای کاربر {1} با موفقیت انجام شد", tour.title, user.PhoneNumber);
            }
            else
            {
                return "اطلاعات ناقص";
            }
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public dynamic Get_Own_Orders()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            string uid = User.Identity.GetUserId();
            var orders = db.Orders.Where(x => x.customer.Id == uid);
            List<dynamic> orderslists = new List<dynamic>();
            foreach (var order in orders)
            {
                dynamic orderslist = new JObject();

                //Order Info
                orderslist.id = order.id;
                orderslist.date = order.date;
                orderslist.price = order.price;
                orderslist.count = order.count.ToString();

                //Payment Info
                orderslist.verificationstatus = order.verificationstatus;
                orderslist.paymentmethod = order.paymentmethod;
                orderslist.authority = order.authority;
                orderslist.sellerid = order.sellerid;
                orderslist.trackid = order.trackid;
                orderslist.status = order.status;
                orderslist.refid = order.refid;

                //Tour Info
                orderslist.tour_id = order.tour.id;
                orderslist.tour_title = order.tour.title;
                orderslist.tour_price = order.tour.price;

                //Customer Info
                orderslist.customer_national_code = order.customer.national_code;
                orderslist.customer_phone_number = order.customer.PhoneNumber;
                orderslist.customer_first_name = order.customer.first_name;
                orderslist.customer_last_name = order.customer.last_name;
                orderslist.customer_id = order.customer.Id;

                orderslists.Add(orderslist);
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(orderslists);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public dynamic Get_Orders()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var orders = db.Orders.ToList();
            List<dynamic> orderslists = new List<dynamic>();
            foreach (var order in orders)
            {
                dynamic orderslist = new JObject();

                //Order Info
                orderslist.id = order.id;
                orderslist.date = order.date;
                orderslist.price = order.price;
                orderslist.count = order.count.ToString();

                //Payment Info
                orderslist.verificationstatus = order.verificationstatus;
                orderslist.paymentmethod = order.paymentmethod;
                orderslist.authority = order.authority;
                orderslist.sellerid = order.sellerid;
                orderslist.trackid = order.trackid;
                orderslist.status = order.status;
                orderslist.refid = order.refid;

                //Tour Info
                orderslist.tour_id = order.tour.id;
                orderslist.tour_title = order.tour.title;
                orderslist.tour_price = order.tour.price;

                //Customer Info
                orderslist.customer_national_code = order.customer.national_code;
                orderslist.customer_phone_number = order.customer.PhoneNumber;
                orderslist.customer_first_name = order.customer.first_name;
                orderslist.customer_last_name = order.customer.last_name;
                orderslist.customer_id = order.customer.Id;

                orderslists.Add(orderslist);
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(orderslists);
        }

        //_______________________________________________________________________________________________________________| .:: توابع فروشنده ها ::.

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<string> Add_Seller(string first_name, string last_name, int gender, string phone_number, string national_code, string pass_word)
        {
            if (!string.IsNullOrWhiteSpace(first_name) &&
                !string.IsNullOrWhiteSpace(last_name) &&
                !string.IsNullOrWhiteSpace(phone_number) &&
                !string.IsNullOrWhiteSpace(national_code) &&
                !string.IsNullOrWhiteSpace(pass_word))
            {
                try
                {
                    var customer = new ApplicationUser()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = "nomail@nomail.com",
                        PhoneNumberConfirmed = true,

                        first_name = first_name.ToString(),
                        last_name = last_name.ToString(),
                        PhoneNumber = phone_number.ToString(),
                        national_code = national_code.ToString(),
                        UserName = phone_number.ToString()
                    };
                    var res = await UserManager.CreateAsync(customer, pass_word);
                    if (res.Succeeded)
                    {
                        var res2 = await UserManager.AddToRoleAsync(customer.Id, "Seller");
                        if (res2.Succeeded)
                        {
                            await new SmsService().SendAsync(new IdentityMessage()
                            {
                                Body = customer.UserName + "%" + pass_word,
                                Destination = phone_number,
                                Subject = Smstemplate.Signup
                            });
                            return string.Format("فروشنده مورد نظر با شماره موبایل {0} افزوده شد", customer.PhoneNumber);
                        }
                        else
                        {
                            return string.Format("فروشنده مورد نظر با شماره موبایل {0} افزوده شد اما نقش مشتری الحاق نشد", customer.PhoneNumber);
                        }
                    }
                    else
                    {
                        return "خطا در افزودن فروشنده";
                    }
                }
                catch
                {
                    return "خطا در افزودن فروشنده";
                }
            }
            else
            {
                return "خطا در افزودن مشتری";
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult Get_Seller()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var users = db.Users.ToList();
            List<ApplicationUser> res = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (UserManager.IsInRole(user.Id, "Seller"))
                {
                    res.Add(user);
                }
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        //_______________________________________________________________________________________________________________| .:: توابع فروشگاه ها ::.

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<string> Add_Provider(string first_name, string last_name, int gender, string phone_number, string national_code, string pass_word)
        {
            if (!string.IsNullOrWhiteSpace(first_name) &&
                !string.IsNullOrWhiteSpace(last_name) &&
                !string.IsNullOrWhiteSpace(phone_number) &&
                !string.IsNullOrWhiteSpace(national_code) &&
                !string.IsNullOrWhiteSpace(pass_word))
            {
                try
                {
                    var customer = new ApplicationUser()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = "nomail@nomail.com",
                        PhoneNumberConfirmed = true,

                        first_name = first_name.ToString(),
                        last_name = last_name.ToString(),
                        PhoneNumber = phone_number.ToString(),
                        national_code = national_code.ToString(),
                        UserName = phone_number.ToString()
                    };
                    var res = await UserManager.CreateAsync(customer, pass_word);
                    if (res.Succeeded)
                    {
                        var res2 = await UserManager.AddToRoleAsync(customer.Id, "Provider");
                        if (res2.Succeeded)
                        {
                            await new SmsService().SendAsync(new IdentityMessage()
                            {
                                Body = customer.UserName + "%" + pass_word,
                                Destination = phone_number,
                                Subject = Smstemplate.Signup
                            });
                            return string.Format("فروشگاه مورد نظر با شماره موبایل {0} افزوده شد", customer.PhoneNumber);
                        }
                        else
                        {
                            return string.Format("فروشگاه مورد نظر با شماره موبایل {0} افزوده شد اما نقش فروشگاه الحاق نشد", customer.PhoneNumber);
                        }
                    }
                    else
                    {
                        return "خطا در افزودن فروشگاه";
                    }
                }
                catch
                {
                    return "خطا در افزودن فروشگاه";
                }
            }
            else
            {
                return "خطا در افزودن فروشگاه";
            }
        }

        [HttpGet]
        [Authorize(Roles = "Provider")]
        public async Task Add_Credit_Orders(string userid, string description, string price)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var pid = User.Identity.GetUserId();
            var user = db.Users.Where(x => x.Id == userid).FirstOrDefault();

            db.Credit_Orders.Add(new Credit_Order()
            {
                id = Guid.NewGuid().ToString(),

                customer = user,
                providerid = pid,
                date = DateTime.Now.ToUniversalTime(),
                description = description,
                price = price
            });

            await db.SaveChangesAsync();
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult Get_Providers()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var users = db.Users.ToList();
            List<ApplicationUser> res = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (UserManager.IsInRole(user.Id, "Provider"))
                {
                    res.Add(user);
                }
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult Get_Credit_Orders()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var users = db.Users.ToList();
            List<ApplicationUser> res = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (UserManager.IsInRole(user.Id, "Provider"))
                {
                    res.Add(user);
                }
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public JsonResult Get_Own_Credit_Orders()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            string uid = User.Identity.GetUserId();
            var res = db.Credit_Orders.Where(x => x.customer.Id == uid);

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Provider")]
        public JsonResult Get_Provider_Credit_Orders()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            string uid = User.Identity.GetUserId();
            var res = db.Credit_Orders.Where(x => x.providerid == uid);

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        private string PasswordGenerator()
        {
            return null;
        }
        #endregion
    }
}