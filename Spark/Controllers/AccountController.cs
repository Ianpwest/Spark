using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spark.Classes;
using Spark.Models;
using System.Web.Security;

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
            if (DatabaseInterface.VerifyAccount(lm.UserName, lm.Password))
            {
                FormsAuthentication.SetAuthCookie(lm.UserName, lm.RememberMe);

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

            //Successfully registered
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
    }
}
