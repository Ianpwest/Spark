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
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using DotNetOpenAuth.AspNet;
using Spark.Filters;

namespace Spark.Controllers
{
    [InitializeSimpleMembership]
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

            //Tell the user to activate
            ViewBag.Activated = "E-mail Sent";
            return View("Activate");
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

        #region External Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            //Address to return to on success
            returnUrl = "/Home";
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));

            string strUserName = result.UserName;
            if (result.Provider == "google" && strUserName.Contains('@'))
                strUserName = strUserName.Substring(0, strUserName.IndexOf('@'));

            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (DatabaseInterface.AccountExists(strUserName))
            {
                FormsAuthentication.SetAuthCookie(strUserName, true);

                //Check to see if the user is activated. If yes set the cookie.
                var useractivatedCookie = new HttpCookie("activated", "True");
                useractivatedCookie.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Set(useractivatedCookie);

                return RedirectToLocal(returnUrl);
            }

            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;

                //If using google instead of using full email address use only first part before @ and send the full name as
                //an email
                if (result.Provider == "google")
                {
                    return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = strUserName, Email = result.UserName, ExternalLoginData = loginData });
                }

                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = strUserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    string strUserName = model.UserName;

                    //Check the user name as the first part of the email
                    if (provider == "google" && strUserName.Contains('@'))
                        strUserName = strUserName.Substring(0, strUserName.IndexOf('@'));

                    // Check if user already exists
                    if (!DatabaseInterface.AccountExists(model.UserName))
                    {
                        // Insert into the accounts table
                        accounts account = new accounts { strUserName = model.UserName, strEmail = model.Email, bIsActivated = true };
                        DatabaseInterface.AddAccount(account);

                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        FormsAuthentication.SetAuthCookie(account.strUserName, true);

                        //Check to see if the user is activated. If yes set the cookie.
                        var useractivatedCookie = new HttpCookie("activated", "True");
                        useractivatedCookie.Expires = DateTime.Now.AddYears(1);
                        Response.Cookies.Set(useractivatedCookie);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        #endregion
    }
}
