using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Models;

namespace Spark.Classes
{
    public class SparksDatabaseInterface
    {
        /// <summary>
        /// Local Instance of the database model
        /// </summary>
        private static Spark.Models.sparkdbEntities m_db = new Models.sparkdbEntities();

        /// <summary>
        /// TODO: Add commenting here
        /// </summary>
        /// <param name="sparkModel"></param>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public static bool CreateSpark(sparks sparkModel, string strUserName)
        {

            var strUser = from r in m_db.accounts
                          join p in m_db.profiles on r.PK equals p.FKAccounts
                          where r.strUserName == strUserName
                          select p.PK;

            if (strUser == null || strUser.Count() != 1)
                return false;

            sparkModel.FKProfilesCreatedBy = strUser.First();

            try
            {
                m_db.AddTosparks(sparkModel);
                m_db.SaveChanges();
            }
            catch (Exception ex)
            {
                // log ex.
                return false;
            }

            return true;
        }
    }
}