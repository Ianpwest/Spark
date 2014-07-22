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
        /// Method to get an instance of a database
        /// </summary>
        /// <returns>database instance</returns>
        public static sparkdbEntities1 GetDatabaseInstance()
        {
            return new sparkdbEntities1();
        }

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
            { 
                LogNonUserError(strMessage, "", "", "", "", "");
                return;
            }
            errorlog log = new errorlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.strMessage = strMessage;

            sparkdbEntities1 db = new sparkdbEntities1();
            db.errorlog.Add(log);
            SaveChanges(db);
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

            sparkdbEntities1 db = new sparkdbEntities1();
            db.errorlog.Add(log);
            SaveChanges(db);

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


            sparkdbEntities1 db = new sparkdbEntities1();
            db.errorlog.Add(log);
            SaveChanges(db);
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

            sparkdbEntities1 db = new sparkdbEntities1();
            m_db.interactionlog.Add(log);
            SaveChanges(db);
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

            sparkdbEntities1 db = new sparkdbEntities1();
            db.interactionlog.Add(log);
            SaveChanges(db);
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

            sparkdbEntities1 db = new sparkdbEntities1();
            db.interactionlog.Add(log);
            SaveChanges(db);
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

            sparkdbEntities1 db = new sparkdbEntities1();
            db.interactionlog.Add(log);
            SaveChanges(db);
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
        public static void LogNonUserError(string strMessage, string strException, string strStackTrace, string strControllerView, string strMethod, string strVariableName)
        {
            errorlog log = new errorlog();
            log.dDate = DateTime.Now;
            log.FKAccounts = int.MinValue;
            log.strMessage = strMessage;
            log.strStackTrace = strStackTrace;
            log.strMethod = strMethod;
            log.strVariableName = strVariableName;

            sparkdbEntities1 db = new sparkdbEntities1();
            db.errorlog.Add(log);
            SaveChanges(db);
        }

        /// <summary>
        /// Attemps to log an interaction that does not have a valid user.
        /// Use empty strings if the string parameters are unknown.
        /// </summary>
        /// <param name="nType"></param>
        /// <param name="strControllerView"></param>
        public static void LogNonUserInteraction(int nType, string strControllerView)
        {
            interactionlog log = new interactionlog();
            log.dDate = DateTime.Now;
            log.FKAccounts = int.MinValue;
            log.FKInteractionTypes = nType;
            log.strControllerView = strControllerView;

            sparkdbEntities1 db = new sparkdbEntities1();
            db.interactionlog.Add(log);
            SaveChanges(db);
        }
        #endregion

        /// <summary>
        /// Base method used to save to a database - should be applied to all derived classes.
        /// Encapsulates the database call in a try catch block.
        /// </summary>
        /// <param name="strUserId">String value of the user Id or an empty string if unknown.</param>
        protected static bool SaveChanges(sparkdbEntities1 database)
        {
            try
            {
                database.GetValidationErrors();
                database.SaveChanges();
            }
            //catch
            //{
            //    // Avoiding for now to prevent recursion on errors.
            //    //LogError(strUserId, "Generate Error", ex.ToString(), ex.StackTrace);
            //    return false;
            //}
            //use this if you're having validation exceptions
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string str =  validationError.PropertyName + validationError.ErrorMessage;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the primary key of the accounts table based on the given parameter for the UserName. Uses int.MinValue as a failure indicator.
        /// </summary>
        /// <param name="db">Database instance that is being used for this call.</param>
        /// <param name="strUserName">String value of the username given by the application's membership.</param>
        /// <returns>Returns the Account's UserId primary key, else returns int.MinValue.</returns>
        public static int GetUserId(sparkdbEntities1 db, string strUserName)
        {
            var qryUserId = from r in db.accounts
                            where r.strUserName == strUserName
                            select r.PK;

            if (qryUserId == null || qryUserId.Count() != 1)
            {
                LogNonUserError("Unable to find username = " + strUserName + " in the database.", "", "", "SparkDatabaseInterface", "UploadArgumentData", "qryUserId");
                return int.MinValue; // failed, returned max value.
            }

            return qryUserId.First();
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