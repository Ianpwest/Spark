using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Classes.DatabaseInterfaces;
using Spark.Models;

namespace Spark.Classes
{
    public class AccountsDatabaseInterface : BaseDatabaseInterface
    {
        /// <summary>
        /// Verifies an account given a login model against the database
        /// </summary>
        /// <param name="lm">Login Model to verify</param>
        /// <returns>Success</returns>
        public static bool VerifyAccount(LoginModel lm)
        {
            bool bExists = false;

            //Get the salt for this user
            string strSaltResult = (from r in m_db.accounts
                                   where r.strUserName == lm.UserName
                                   select r.strSalt).FirstOrDefault();

            //Get the hash for this user given what they typed for their password and their salt
            string strHashToCheck = Utilities.GetHashPassword(lm.Password, strSaltResult);

            //See if there is a user with this username and password; return 
            var result =   from r in m_db.accounts
                           where r.strUserName == lm.UserName
                           && r.strPassword == strHashToCheck
                           select r.bIsActivated;
            
            //We have a user
            if (result.Count() > 0)
            {
                lm.bIsActivated = result.FirstOrDefault();
                return true;
            }

            return bExists;
        }

        /// <summary>
        /// Checks to see if an account exists given a username
        /// </summary>
        /// <param name="strUserName">username to check</param>
        /// <returns>If account already exists</returns>
        public static bool AccountUsernameExists(string strUserName)
        {
            bool bExists = false;

            int results = (from r in m_db.accounts
                           where r.strUserName == strUserName
                           select r).Count();

            if (results != 0)
                return true;

            return bExists;
        }

        /// <summary>
        /// Method to reset the password of an account to the specified new password
        /// </summary>
        /// <param name="pcm">Password Change Model</param>
        /// <returns>New Password</returns>
        public static bool ResetAccountPassword(PasswordChangeModel pcm)
        {
            if (pcm == null)
                return false;

            accounts account = GetAccount(pcm.Username);

            if (account == null)
                return false;
            
            account.strSalt = Utilities.GetSalt();
            account.strPassword = Utilities.Encrypt(account.strSalt + pcm.NewPassword);

            try
            {
                //Save the changes
                m_db.SaveChanges();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method to send the reset account password email to the user to begin
        /// the account reset process
        /// </summary>
        /// <param name="strEmail">E-mail of person to reset account</param>
        /// <returns>Success</returns>
        public static bool ResetAccountSendEmail(string strEmail)
        {
            if (string.IsNullOrEmpty(strEmail))
                return false;

            if (AccountEmailExists(strEmail))
            {
                //Get the account only if they are using an account through spark not oauth
                accounts accountReset = (from r in m_db.accounts
                                         where r.strEmail == strEmail
                                         && r.gActivationGUID != null
                                         select r).FirstOrDefault();

                if (accountReset == null)
                    return false;

                //Use their current activation guid to uniquely identify them.
                string strMessage = "Click on this link to reset your password: http://localhost:51415/Account/ChangePassword?user=" + accountReset.gActivationGUID;
                Utilities.SendEmail(accountReset.strEmail, "Account Recovery", strMessage);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if an account exists given an email
        /// </summary>
        /// <param name="strUserName">username to check</param>
        /// <returns>If account already exists</returns>
        public static bool AccountEmailExists(string strEmail)
        {
            bool bExists = false;

            int results = (from r in m_db.accounts
                           where r.strEmail == strEmail
                           select r).Count();

            if (results != 0)
                return true;

            return bExists;
        }

        /// <summary>
        /// Adds an account to the database
        /// </summary>
        /// <param name="account">Account to add</param>
        /// <returns>Success</returns>
        public static bool AddAccount(accounts account)
        {
            try
            {
                m_db.accounts.Add(account);
                m_db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to register an account given a registration model
        /// </summary>
        /// <param name="rm">Register Model to register</param>
        /// <returns>Success</returns>
        public static bool RegisterAccount(Spark.Models.RegisterModel rm)
        {
            //Create a new account model to add to the database with the given information
            Models.accounts accountNew = new Models.accounts();

            accountNew.strUserName = rm.UserName;
            accountNew.strSalt = Utilities.GetSalt();
            accountNew.strPassword = Utilities.Encrypt(accountNew.strSalt + rm.Password);
            accountNew.gActivationGUID = rm.gActivationGUID;
            accountNew.strEmail = rm.Email;

            try
            {
                m_db.accounts.Add(accountNew);
                m_db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the database to see if the user is activating with the guid that is
        /// associated with their account.
        /// </summary>
        /// <param name="userGUID">Guid from email link</param>
        public static bool ActivateAccount(string userGUID)
        {
            accounts result =  (from r in m_db.accounts
                                where r.gActivationGUID.ToString() == userGUID
                                select r).FirstOrDefault();

            //User not found
            if (result == null)
                return false;

            //Failed to provide the right activation token
            if(result.gActivationGUID.ToString() != userGUID)
                return false;

            //Set the user to activated
            result.bIsActivated = true;

            try
            {
                m_db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets an account given a username
        /// </summary>
        /// <param name="strUserName">username of account</param>
        /// <returns>Account</returns>
        public static accounts GetAccount(string strUserName)
        {
            accounts account = (from r in m_db.accounts
                               where r.strUserName == strUserName
                               select r).FirstOrDefault();

            if (account == null)
                return null;

            return account;
        }

        /// <summary>
        /// Gets an account given a salt
        /// </summary>
        /// <param name="strUserName">salt of account</param>
        /// <returns>Account</returns>
        public static accounts GetAccountByActivationGuid(string strActivationGUID)
        {
            accounts account = (from r in m_db.accounts
                                where r.gActivationGUID.ToString() == strActivationGUID
                                select r).FirstOrDefault();

            if (account == null)
                return null;

            return account;
        }
    }
}