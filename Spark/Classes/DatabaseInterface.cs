using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spark.Classes
{
    public static class DatabaseInterface
    {
        /// <summary>
        /// Local Instance of the database model
        /// </summary>
        private static Spark.Models.sparkdbEntities m_db = new Models.sparkdbEntities();

        ///// <summary>
        ///// Gets all events in the database
        ///// </summary>
        ///// <returns>List of all events</returns>
        //public static List<Models.events> GetAllEvents()
        //{
        //    List<Eventcity.Models.events> lstEvents = new List<Models.events>();

        //    //Get all the events from the database
        //    var results = from r in m_db.events
        //                  select r;

        //    //Add them to a list to pass to the view
        //    foreach (Models.events ev in results)
        //    {
        //        lstEvents.Add(ev);

        //    }

        //    return lstEvents;
        //}


        public static bool VerifyAccount(string strUserName, string strPassword)
        {
            bool bExists = false;

            //Get the salt for this user
            string strSaltResult = (from r in m_db.accounts
                                   where r.strUserName == strUserName
                                   select r.strSalt).FirstOrDefault();

            //Get the hash for this user given what they typed for their password and their salt
            string strHashToCheck = Utilities.GetHashPassword(strPassword, strSaltResult);

            int results = (from r in m_db.accounts
                           where r.strUserName == strUserName
                           && r.strPassword == strHashToCheck
                           select r).Count();

            if (results != 0)
                return true;

            return bExists;
        }

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

        public static bool RegisterAccount(Spark.Models.RegisterModel rm)
        {
            //Create a new account model to add to the database with the given information
            Models.accounts accountNew = new Models.accounts();

            accountNew.strUserName = rm.UserName;
            accountNew.strSalt = Utilities.GetSalt();
            accountNew.strPassword = Utilities.Encrypt(accountNew.strSalt + rm.Password);

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

    }
}