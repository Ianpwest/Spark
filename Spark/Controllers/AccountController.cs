using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spark.Classes;
using Spark.Models;
using System.Web.Security;
using System.Net.Mail;
using System.Net;

namespace Spark.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        public ActionResult Index()
        {
            return View();
        }


        //
        // GET: /Login/
        public ActionResult Login()
        {
            //Pass in empty login model
            Models.LoginModel lm = new Models.LoginModel();

            return View(lm);
        }

        [HttpPost]
        public ActionResult Login(Spark.Models.LoginModel lm)
        {
            //Verify the account exists with the given information
            if (DatabaseInterface.VerifyAccount(lm))
            {
                FormsAuthentication.SetAuthCookie(lm.UserName, lm.RememberMe);
               
                //Check to see if the user is activated. If yes set the cookie.
                var useractivatedCookie = new HttpCookie("activated", lm.bIsActivated.ToString());
                useractivatedCookie.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Set(useractivatedCookie);
                
                return RedirectToAction("Index", "Home");
            }

            //Failed to login
            lm.bFailedLogin = true;

            return View(lm);
        }

        //
        // GET: /Register/
        public ActionResult Register()
        {
            Models.RegisterModel rm = new Models.RegisterModel();

            return View(rm);
        }

        [HttpPost]
        public ActionResult Register(Models.RegisterModel rm)
        {
            //Check to see if the account name is already in use
            if (DatabaseInterface.AccountExists(rm.UserName))
            {
                rm.bFailedRegister = true;
                return View(rm);
            }

            //If database failed to register
            if (!DatabaseInterface.RegisterAccount(rm))
            {
                rm.bFailedRegister = true;
                return View(rm);
            }

            SendActivationEmail(rm.Email, rm.gActivationGUID.ToString());

            FormsAuthentication.SetAuthCookie(rm.UserName, false);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // GET: /Activate/
        public ActionResult Activate(string user)
        {
            //Check to see if the guid given is the guid associated with this account.
            if (!DatabaseInterface.ActivateAccount(user, User.Identity.Name))
            {
                //We failed activation do something here... probably need to send them another e-mail to try again.
                ViewBag.Activated = "False";
                return View();
            }

            //The user has been activated. Set their activated cookie to true
            var useractivatedCookie = new HttpCookie("activated", "true");
            useractivatedCookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Set(useractivatedCookie);

            ViewBag.Activated = "True";
            return View();
        }

        private void SendActivationEmail(string strEmail, string strActivationGUID)
        {
            //Successfully registered
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("SparkItEmail@gmail.com", "Margaritas!"),
                EnableSsl = true
            };


            client.Send("SparkItEmail@gmail.com", strEmail, "Activate your account", 
                        "Please click the link below to activate your account: \r\n\r\n http://localhost:51415/Account/Activate?user=" + strActivationGUID);
        }

        public ActionResult ResendActivationEmail()
        {
            accounts account = DatabaseInterface.GetAccount(User.Identity.Name);

            if (account == null)
            {
                ViewBag.Activated = "No Account Found";
                return View("Activate");
            }

            SendActivationEmail(account.strEmail, account.gActivationGUID.ToString());
            ViewBag.Activated = "E-mail Sent";
            return View("Activate");
        }
    }
}
