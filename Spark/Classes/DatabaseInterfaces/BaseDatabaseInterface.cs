using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Models;

namespace Spark.Classes.DatabaseInterfaces
{
    /// <summary>
    /// Base class to be used for all database interface classes. This class contains the static sparkdbEntities field.
    /// This class also contains all error logging and user interaction logging.
    /// </summary>
    public class BaseDatabaseInterface
    {
        /// Logs an error in the database using the given account Id, custom message, stacktrace message, calling controller or view, calling method, and potentially errored variable field.
        /// If a parameter is unknown or unused, use an empty string or null.
        protected static sparkdbEntities m_db = new sparkdbEntities();

        /// <summary>
        /// Logs an error in the database using the given account Id and custom message.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="strMessage">Custom message about error to be stored.</param>
        public static void LogError(int accountId, string strMessage)
        {
            DateTime dtNow = DateTime.Now;

            errorlog log = new errorlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.strMessage = strMessage;

            m_db.errorlog.Add(log);
            m_db.SaveChanges();
        }

        /// <summary>
        /// Logs an error in the database using the given account Id, custom message, and stacktrace message.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="strMessage">Custom message about error to be stored.</param>
        /// <param name="strStackTrace">Given by stacktrace of the exception.</param>
        public static void LogError(int accountId, string strMessage, string strStackTrace)
        {
            DateTime dtNow = DateTime.Now;

            errorlog log = new errorlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.strMessage = strMessage;
            log.strStackTrace = strStackTrace;

            m_db.errorlog.Add(log);
            m_db.SaveChanges();

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
        public static void LogError(int accountId, string strMessage, string strControllerView, string strMethod, string strStackTrace, string strVariableName)
        {
            DateTime dtNow = DateTime.Now;

            errorlog log = new errorlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.strMessage = strMessage;
            log.strStackTrace = strStackTrace;
            log.strMethod = strMethod;
            log.strVariableName = strVariableName;

            m_db.errorlog.Add(log);
            m_db.SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id and the interaction type.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        public static void LogInteraction(int accountId, InteractionType type)
        {
            DateTime dtNow = DateTime.Now;

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = (int)type;

            m_db.interactionlog.Add(log);
            m_db.SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id and the interaction type. Uses an integer to log the type instead of enumeration.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        public static void LogInteraction(int accountId, int type)
        {
            DateTime dtNow = DateTime.Now;

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = type;

            m_db.interactionlog.Add(log);
            m_db.SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id, the interaction type, and the calling controller or view name.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        /// <param name="strControllerView">Controller or view that originated the interaction.</param>
        public static void LogInteraction(int accountId, InteractionType type, string strControllerView)
        {
            DateTime dtNow = DateTime.Now;

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = (int)type;
            log.strControllerView = strControllerView;

            m_db.interactionlog.Add(log);
            m_db.SaveChanges();
        }

        /// <summary>
        /// Logs a user interaction using the account Id, the interaction type, and the calling controller or view name. Uses an integer to log the type instead of enumeration.
        /// </summary>
        /// <param name="accountId">Account Id</param>
        /// <param name="type">Type of user interaction as specificed in the database configuration table.</param>
        /// <param name="strControllerView">Controller or view that originated the interaction.</param>
        public static void LogInteraction(int accountId, int type, string strControllerView)
        {
            DateTime dtNow = DateTime.Now;

            interactionlog log = new interactionlog();
            log.dDate = dtNow;
            log.FKAccounts = accountId;
            log.FKInteractionTypes = type;
            log.strControllerView = strControllerView;

            m_db.interactionlog.Add(log);
            m_db.SaveChanges();
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