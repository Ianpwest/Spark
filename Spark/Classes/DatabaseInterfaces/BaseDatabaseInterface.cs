using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spark.Classes.DatabaseInterfaces
{
    public static class BaseDatabaseInterface
    {
        // TODO - change this to the error logging entity model when the model is updated from the DB.
        private static Spark.Models.sparkdbEntities m_dbErrorLog = new Models.sparkdbEntities();

        public static void LogError(int accountId, string strMessage)
        {
            DateTime dtNow = DateTime.Now;
        }

        public static void LogError(int accountId, string strMessage, string strStackTrace)
        {
            DateTime dtNow = DateTime.Now;
        }

        public static void LogError(int accountId, string strMessage, string strControllerView, string strMethod, string strStackTrace, string strVariableName)
        {
            DateTime dtNow = DateTime.Now;
        }
    }
}