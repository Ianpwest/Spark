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
                //Set the authorization cookie with the username
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
            if (DatabaseInterface.AccountUsernameExists(rm.UserName))
            {
                rm.bFailedRegister = true;

                ViewBag.Status = "Username has already been taken.";
                return View(rm);
            }

            //Check to see if the email is already in use
            if (DatabaseInterface.AccountEmailExists(rm.Email))
            {
                rm.bFailedRegister = true;

                ViewBag.Status = "Email has already been registered.";
                return View(rm);
            }

            //If database failed to register
            if (!DatabaseInterface.RegisterAccount(rm))
            {
                rm.bFailedRegister = true;
                return View(rm);
            }

            //Sends an email for the user to verify that they have a valid email address before they 
            //are allowed to contribute to the site.

            string strMessage = "Please click the link below to activate your account: \r\n\r\n http://localhost:51415/Account/Activate?user=" + rm.gActivationGUID.ToString();
            if (Utilities.SendEmail(rm.Email, "Activate your account", strMessage))
                ViewBag.Activated = "E-mail Sent";
            else
                ViewBag.Activated = "False";

            //Set authorization cookie
            FormsAuthentication.SetAuthCookie(rm.UserName, false);

            return View("Activate");
        }

        /// <summary>
        /// Get: Start Account retrieval process
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountRetrieval()
        {
            return View();
        }

        /// <summary>
        /// Post: Account retrieval.
        /// </summary>
        /// <param name="ar">Account retrieval model</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult AccountRetrieval(Spark.Models.AccountRetrieval ar)
        {
            if (DatabaseInterface.ResetAccountSendEmail(ar.Email))
                ViewBag.Status = "Email Sent";
            else
                ViewBag.Status = "Failure-AccountDoesNotExist";

           
            return View();
        }

        /// <summary>
        /// Get: Change Password. Started from email link
        /// </summary>
        /// <param name="user">User Activation Guid (Used to uniquely identify user)</param>
        /// <returns>View</returns>
        public ActionResult ChangePassword(string user)
        {
            //Get the account
            accounts accountReset = DatabaseInterface.GetAccountByActivationGuid(user);
            
            Models.PasswordChangeModel pcm = new PasswordChangeModel();
            pcm.Username = accountReset.strUserName;

            ViewBag.Username = accountReset.strUserName;

            return View(pcm);
        }

        /// <summary>
        /// Post: Actually resets the password to the given parameters specified in the password change model
        /// </summary>
        /// <param name="pcm">Password Change Model</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult ChangePassword(PasswordChangeModel pcm)
        {
            if (DatabaseInterface.ResetAccountPassword(pcm))
                ViewBag.Status = "Success";
            else
                ViewBag.Status = "Failure";

            return View("AccountRetrieval");
        }

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

            //Use the viewbag to let the view know which html to display
            ViewBag.Activated = "True";
            return View();
        }

        /// <summary>
        /// Method to send an email from our gmail account. Credentials are inside this method for 
        /// the account.
        /// </summary>
        /// <param name="strEmail">E-mail to send to</param>
        /// <param name="strActivationGUID">Guid to send the user to activate against</param>
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

        /// <summary>
        /// Method to resend the activation e-mail if the user did not receive it.
        /// </summary>
        /// <returns>View notifying the user</returns>
        public ActionResult ResendActivationEmail()
        {
            accounts account = DatabaseInterface.GetAccount(User.Identity.Name);

            if (account == null)
            {
                ViewBag.Activated = "No Account Found";
                return View("Activate");
            }

            string strMessage = "Please click the link below to activate your account: \r\n\r\n http://localhost:51415/Account/Activate?user=" + account.gActivationGUID.ToString();

            if (Utilities.SendEmail(account.strEmail, "Activate your account", strMessage))
                ViewBag.Activated = "E-mail Sent";
            else
                ViewBag.Activated = "False";

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

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));

            string strUserName = result.UserName;

            //We don't want their email address to be their default username so we extract the first part of their email to 
            //provide as a default. This only apples to google logins fb defaults fine.
            if (result.Provider == "google" && strUserName.Contains('@'))
                strUserName = strUserName.Substring(0, strUserName.IndexOf('@'));

            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }
            //This user already has an account registered with us, go ahead and log them in
            if (DatabaseInterface.AccountUsernameExists(strUserName))
            {
                FormsAuthentication.SetAuthCookie(strUserName, true);

                //Check to see if the user is activated. If yes set the cookie.
                var useractivatedCookie = new HttpCookie("activated", "True");
                useractivatedCookie.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Set(useractivatedCookie);

                return RedirectToLocal(returnUrl);
            }
            //The user has authenticated with the external provider but does not have an account registered with us.
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
                string strUserName = model.UserName;

                //Check the user name as the first part of the email
                if (provider == "google" && strUserName.Contains('@'))
                    strUserName = strUserName.Substring(0, strUserName.IndexOf('@'));

                // Check if user already exists
                if (!DatabaseInterface.AccountUsernameExists(model.UserName))
                {
                    // Insert into the accounts table
                    accounts account = new accounts { strUserName = model.UserName, strEmail = model.Email, bIsActivated = true };
                    DatabaseInterface.AddAccount(account);

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
