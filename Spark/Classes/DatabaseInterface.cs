using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Models;

namespace Spark.Classes
{
    public static class DatabaseInterface
    {
        /// <summary>
        /// Local Instance of the database model
        /// </summary>
        private static Spark.Models.sparkdbEntities m_db = new Models.sparkdbEntities();

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
        public static bool AccountExists(string strUserName)
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
        /// Adds an account to the database
        /// </summary>
        /// <param name="account">Account to add</param>
        /// <returns>Success</returns>
        public static bool AddAccount(accounts account)
        {
            try
            {
                m_db.accounts.AddObject(account);
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
            accountNew.gActivationGUID = rm.gActivationGUID.ToString();
            accountNew.strEmail = rm.Email;

            try
            {
                m_db.AddToaccounts(accountNew);
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
        public static bool ActivateAccount(string userGUID, string strUserName)
        {
            accounts result =  (from r in m_db.accounts
                                where r.strUserName == strUserName
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

        internal static void CreateSpark(sparks sparkModel)
        {
            // Do stuff
            sparks sparkNew = new sparks();

            sparkNew.strImagePath = sparkModel.strImagePath;
            sparkNew.strDescription = sparkModel.strDescription;
            sparkNew.strTopic = sparkModel.strTopic;
            sparkNew.FKSubjectMatters = sparkModel.FKSubjectMatters;
            sparkNew.FKProfilesCreatedBy = sparkModel.FKProfilesCreatedBy;
            try
            {
                m_db.AddTosparks(sparkNew);
                m_db.SaveChanges();
            }
            catch (Exception ex)
            {
                // log ex.
            }
        }
    }
}