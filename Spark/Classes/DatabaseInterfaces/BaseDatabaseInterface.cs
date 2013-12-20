using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using Spark.Models;

namespace Spark.Classes.DatabaseInterfaces
{
    /// <summary>
    /// Base class to be used for all database interface classes. This class contains the static sparkdbEntities1 field.
    /// This class also contains all error logging and user interaction logging.
    /// </summary>
    public class BaseDatabaseInterface
    {
        #region Logging

        /// Logs an error in the database using the given account Id, custom message, stacktrace message, calling controller or view, calling method, and potentially errored variable field.
        /// If a parameter is unknown or unused, use an empty string or null.
        protected static sparkdbEntities1 m_db = new sparkdbEntities1();

        /// <summary>
        /// Logs an error in the database using the given account Id and custom message.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="strMessage">Custom message about error to be stored.</param>
        public static void LogError(string strUserName, string strMessage)
        {
            DateTime dtNow = DateTime.Now;
            int accountId = GetUserId(strUserName);

            if (accountId == int.MinValue)
                LogNonUserError(strMessage, "", "", "", "", "");
            errorlog log = new errorlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.strMessage = strMessage;

            m_db.errorlog.Add(log);
            SaveChanges();
        }

        /// <summary>
        /// Logs an error in the database using the given account Id, custom message, and stacktrace message.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="strMessage">Custom message about error to be stored.</param>
        /// <param name="strStackTrace">Given by stacktrace of the exception.</param>
        public static void LogError(string strUserName, string strMessage, string strException, string strStackTrace)
        {
            DateTime dtNow = DateTime.Now;
            int accountId = GetUserId(strUserName);

            if (accountId == int.MinValue)
                LogNonUserError(strMessage, strException, strStackTrace, "", "", "");

            errorlog log = new errorlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.strMessage = strMessage;
            log.strStackTrace = strStackTrace;

            m_db.errorlog.Add(log);
            SaveChanges();

        }

        /// <summary>
        /// Logs an error in the database using the given account Id, custom message, stacktrace message, calling controller or view, calling method, and potentially errored variable field.
        /// If a parameter is unknown or unused, use an empty string or null.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="strMessage">Custom message about error to be stored.</param>
        /// <param name="strControllerView">Controller or view on which the error is occuring.</param>
        /// <param name="strMethod">Method that contains the error.</param>
        /// <param name="strStackTrace">Given by stacktrace of the exception.</param>
        /// <param name="strVariableName">Variable name that could possibly be null or otherwise error prone.</param>
        public static void LogError(string strUserName, string strMessage, string strException, string strStackTrace, string strControllerView, string strMethod, string strVariableName)
        {
            DateTime dtNow = DateTime.Now;
            int accountId = GetUserId(strUserName);

            if (accountId == int.MinValue)
                LogNonUserError(strMessage, strException, strStackTrace, strControllerView, strMethod, strVariableName);

            errorlog log = new errorlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.strMessage = strMessage;
            log.strStackTrace = strStackTrace;
            log.strMethod = strMethod;
            log.strVariableName = strVariableName;

            m_db.errorlog.Add(log);
            SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id and the interaction type.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        public static void LogInteraction(string strUserName, InteractionType type)
        {
            DateTime dtNow = DateTime.Now;
            int accountId = GetUserId(strUserName);
            if (accountId == int.MinValue)
                LogNonUserInteraction((int)type, "");

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = (int)type;

            m_db.interactionlog.Add(log);
            SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id and the interaction type. Uses an integer to log the type instead of enumeration.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        public static void LogInteraction(string strUserName, int type)
        {
            DateTime dtNow = DateTime.Now;
            int accountId = GetUserId(strUserName);

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = type;

            m_db.interactionlog.Add(log);
            SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id, the interaction type, and the calling controller or view name.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        /// <param name="strControllerView">Controller or view that originated the interaction.</param>
        public static void LogInteraction(string strUserName, InteractionType type, string strControllerView)
        {
            DateTime dtNow = DateTime.Now;
            int accountId = GetUserId(strUserName);

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = (int)type;
            log.strControllerView = strControllerView;

            m_db.interactionlog.Add(log);
            SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id, the interaction type, and the calling controller or view name. Uses an integer to log the type instead of enumeration.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        /// <param name="strControllerView">Controller or view that originated the interaction.</param>
        public static void LogInteraction(string strUserName, int type, string strControllerView)
        {
            DateTime dtNow = DateTime.Now;
            int accountId = GetUserId(strUserName);

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = type;
            log.strControllerView = strControllerView;

            m_db.interactionlog.Add(log);
            SaveChanges();
        }
        
        /// <summary>
        /// Attempts to find the user identity PK from the database that is associated with the given username stored by microsoft's built in User framework.
        /// Returns the numeric Id or int.MinValue if no Id can be found.
        /// </summary>
        /// <param name="strUserName">Username given by the User.Identity.Name from a controller class.</param>
        /// <returns>Database keyd Id number or int.MinValue if no Id can be found.</returns>
        private static int GetUserId(string strUserName)
        {
            var qryForId = from r in m_db.accounts
                           where r.strUserName == strUserName
                           select r.PK;

            if (qryForId != null && qryForId.Count() > 0)
                return qryForId.FirstOrDefault();

            return int.MinValue;
        }

        /// <summary>
        /// Attempts to log an error that does not have a valid user.
        /// Use empty strings if the string parameters are unknown.
        /// </summary>
        private static void LogNonUserError(string strMessage, string strException, string strStackTrace, string strControllerView, string strMethod, string strVariableName)
        {
            errorlog log = new errorlog();
            log.dDate = DateTime.Now;
            log.FKAccounts = int.MinValue;
            log.strMessage = strMessage;
            log.strStackTrace = strStackTrace;
            log.strMethod = strMethod;
            log.strVariableName = strVariableName;

            m_db.errorlog.Add(log);
            SaveChanges();
        }

        /// <summary>
        /// Attemps to log an interaction that does not have a valid user.
        /// Use empty strings if the string parameters are unknown.
        /// </summary>
        /// <param name="nType"></param>
        /// <param name="strControllerView"></param>
        private static void LogNonUserInteraction(int nType, string strControllerView)
        {
            interactionlog log = new interactionlog();
            log.dDate = DateTime.Now;
            log.FKAccounts = int.MinValue;
            log.FKInteractionTypes = nType;
            log.strControllerView = strControllerView;

            m_db.interactionlog.Add(log);
            SaveChanges();
        }
        #endregion

        /// <summary>
        /// Base method used to save to a database - should be applied to all derived classes.
        /// Encapsulates the database call in a try catch block.
        /// </summary>
        /// <param name="strUserId">String value of the user Id or an empty string if unknown.</param>
        protected static bool SaveChanges()
        {
            try
            {
                m_db.SaveChanges();
            }
            catch
            {
                // Avoiding for now to prevent recursion on errors.
                //LogError(strUserId, "Generate Error", ex.ToString(), ex.StackTrace);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Enumeration that correlates to the database's interaction type configuration table. Specifies which type of user interaction occurs for the log record.
    /// </summary>
    public enum InteractionType
    {
        Unknown = 0,
        Login = 1,
        SubmitForm = 2
    }
}