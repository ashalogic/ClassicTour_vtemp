using ClassicTour_vtemp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ClassicTour_vtemp.Controllers
{

    public class HomeController : Controller
    {
        zarinpal.PaymentGatewayImplementationServicePortTypeClient paymentobj = new zarinpal.PaymentGatewayImplementationServicePortTypeClient();

        private readonly string MCode = "63e9f2c0-ef70-11e8-b14c-005056a205be";
        private readonly string CallBackURL = "http://www.classictour.ir/home/verify";
        private readonly string ZarinpalURL = "https://zarinpal.com/pg/StartPay/";

        //private readonly string ZarinpalURL = "https://sandbox.zarinpal.com/pg/StartPay/";
        //sandbox_zarinpal.PaymentGatewayImplementationServicePortTypeClient paymentobj = new sandbox_zarinpal.PaymentGatewayImplementationServicePortTypeClient();

        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                ApplicationDbContext db = new ApplicationDbContext();
                string uid = User.Identity.GetUserId();
                var user = db.Users.Where(x => x.Id == uid).FirstOrDefault();
                if (user != null)
                {
                    ViewBag.cinfo_email = user.Email;
                    ViewBag.cinfo_firstname = user.first_name;
                    ViewBag.cinfo_lastname = user.last_name;
                    ViewBag.cinfo_phonenumber = user.PhoneNumber;
                }
            }

            ViewBag.Index = "active";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.About = "active";
            return View();
        }

        public ActionResult Terms()
        {
            ViewBag.Terms = "active";
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult Get_Tours()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var tours = db.Tours.Where(x => x.status == true).ToList();
            return Json(tours, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public async Task<string> Buy(string token, string id, int count, string refid)
        {
            //Check Recaptcha
            HttpClient client = new HttpClient();
            var results = await client.GetStringAsync(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", "6Le8AnkUAAAAAD2c7LMUow0ElJi6Yu-xOIusKgyJ", token));
            var obj = JObject.Parse(results);
            var status = (bool)obj.SelectToken("success");
            if (status)
            {
                if ((double)obj.SelectToken("score") >= 0.6)
                {
                    //Find Tour
                    ApplicationDbContext db = new ApplicationDbContext();
                    var tour = db.Tours.Where(x => x.id == id).FirstOrDefault();

                    //Find User
                    string uid = User.Identity.GetUserId();
                    var user = db.Users.Where(x => x.Id == uid).FirstOrDefault();

                    if (tour != null && tour.capacity >= count && user != null)
                    {
                        int res = paymentobj.PaymentRequest(MCode, int.Parse(tour.price) * count, tour.title, "", User.Identity.GetUserName(), CallBackURL, out string Authority);
                        switch (res)
                        {
                            case -1:
                                return "اطلاعات ارسال شده ناقص می باشد";
                            case -2:
                                return "ادرس IP و یا مرچنت کد به صورت صحیح وارد نشده";
                            case -3:
                                return "حداقل مبلغ قابل پرداخت 100 تومان می باشد";
                            case -4:
                                return "سطح تایید پذیرنده پایین تر از سطح نقره ای است";
                            case 100:
                                var order = new Order
                                {
                                    id = Guid.NewGuid().ToString(),

                                    tour = tour,
                                    customer = user,

                                    price = (int.Parse(tour.price) * count).ToString(),
                                    authority = Authority,
                                    count = count,

                                    date = DateTime.Now.ToUniversalTime(),
                                    paymentmethod = PaymentMethod.Online,
                                    verificationstatus = VerificationStatus.notverified
                                };
                                user.Orders.Add(order);
                                var result = db.SaveChanges();
                                if (result == 3)
                                    return ZarinpalURL + Authority;
                                else
                                    return "خطا در ثبت اطلاعات";
                            default:
                                return "خطا در اتصال به درگاه پرداخت زرین پال";
                        }
                    }
                    else
                        return "تور مورد نظر یافت نشد !";
                }
                else
                    return "عدم تایید توسط کپچا";
            }
            else
                return "خطا در ارتباط با کپچا";
        }

        [Authorize]
        [HttpGet]
        public ActionResult Verify(string Authority, string Status)
        {
            //Setting Result Message
            ViewBag.RefID = "";
            ViewBag.Message = "";
            ViewBag.MessageColor = "";
            ViewBag.MessageStatusCode = "";

            //Check Authority Style
            if (Authority.Length == 36 && long.TryParse(Authority, out long longAuthority))
            {
                //Check Authority with Order
                ApplicationDbContext db = new ApplicationDbContext();
                var order = db.Orders.Where(x => x.authority == Authority).FirstOrDefault();

                if (order != null)
                {
                    //Check Payment Verification
                    if (Status == "OK")
                    {
                        int res = paymentobj.PaymentVerification(MCode, order.authority, int.Parse(order.price), out long refid);
                        //Update refid
                        order.refid = refid.ToString();
                        order.verificationstatus = VerificationStatus.verified;

                        //Check result message
                        switch (res)
                        {
                            case -1:
                                ViewBag.Message = "اطلاعات ارسال شده ناقص می باشد";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-1";
                                break;
                            case -2:
                                ViewBag.Message = "ادرس IP و یا مرچنت کد به صورت صحیح وارد نشده";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-2";
                                break;
                            case -11:
                                ViewBag.Message = "ﺩﺭﺧﻮﺍﺳﺖ ﻣﻮﺭﺩ ﻧﻈﺮ ﻳﺎﻓﺖ ﻧﺸﺪ";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-11";
                                break;
                            case -21:
                                ViewBag.Message = "ﻫﻴﭻ ﻧﻮﻉ ﻋﻤﻠﻴﺎﺕ ﻣﺎﻟﻲ ﺑﺮﺍﻱ ﺍﻳﻦ ﺗﺮﺍﻛﻨﺶ ﻳﺎﻓﺖ ﻧﺸﺪ";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-21";
                                break;
                            case -22:
                                ViewBag.Message = "ﺗﺮﺍﻛﻨﺶ ﻧﺎ ﻣﻮﻓﻖ ﻣﻲﺑﺎﺷﺪ";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-22";
                                break;
                            case -33:
                                ViewBag.Message = "ﺭﻗﻢ ﺗﺮﺍﻛﻨﺶ ﺑﺎ ﺭﻗﻢ ﭘﺮﺩﺍﺧﺖ ﺷﺪﻩ ﻣﻄﺎﺑﻘﺖ ﻧﺪﺍﺭﺩ";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-33";
                                break;
                            case -42:
                                ViewBag.Message = "مدت زمان معتبر طول عمر شناسه پرداخت باید بین 30 دقیقه تا 45 روز باشد";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-42";
                                break;
                            case -54:
                                ViewBag.Message = "درخواست مورد نظر آرشیو شد است";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = "-54";
                                break;
                            case 100:
                                ViewBag.refid = refid.ToString();
                                ViewBag.Message = "تراکنش با موفقیت انجام شد";
                                ViewBag.MessageColor = "alert-success";
                                order.tour.capacity--;
                                new SmsService().Send(new IdentityMessage() { Subject = Smstemplate.Payment_Done, Body = order.price, Destination = User.Identity.GetUserName() });
                                ViewBag.MessageStatusCode = "100";
                                order.status = true;
                                break;
                            case 101:
                                ViewBag.refid = refid.ToString();
                                ViewBag.Message = "عملیات پرداخت موفق بوده و قبلا تایید شده است";
                                ViewBag.MessageColor = "alert-warning";
                                ViewBag.MessageStatusCode = "101";
                                break;
                            default:
                                ViewBag.Message = "خطای پیش بینی نشده";
                                ViewBag.MessageColor = "alert-danger";
                                ViewBag.MessageStatusCode = res;
                                break;
                        }

                        //Save Changes
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.Message = "تراکنش با موفقیت انجام نشد";
                        ViewBag.MessageColor = "alert-danger";
                        ViewBag.MessageStatusCode = "";
                    }
                }
                else
                {
                    ViewBag.Message = "تراکنش مورد نظر یافت نشد";
                    ViewBag.MessageColor = "alert-danger";
                    ViewBag.MessageStatusCode = "";
                }
            }
            return View();
        }
    }
}