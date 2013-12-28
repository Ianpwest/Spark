using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using Spark.Classes.DatabaseInterfaces;
using Spark.Models;

namespace Spark.Classes
{
    public class SparksDatabaseInterface : BaseDatabaseInterface
    {
        /// <summary>
        /// TODO: Add commenting here
        /// </summary>
        /// <param name="sparkModel"></param>
        /// <param name="strUserName"></param>
        /// <returns></returns>
        public static bool CreateSpark(SparkCreateModel sparkCreateModel)
        {
            sparks sparkModel = new sparks();
            sparkModel.strDescription = sparkCreateModel.Description;
            sparkModel.strTopic = sparkCreateModel.Topic;
            
            sparkModel.FKSubjectMatters = sparkCreateModel.SubjectMatterId;
            
            var nQryUserId = from r in m_db.accounts
                             where r.strUserName == sparkCreateModel.UserId
                             select r.PK;

            if (nQryUserId == null || nQryUserId.Count() != 1)
                return false;

            sparkModel.FKCategories1 = sparkCreateModel.Tag1;
            sparkModel.FKCategories2 = sparkCreateModel.Tag2;
            sparkModel.FKCategories3 = sparkCreateModel.Tag3;
            sparkModel.FKCategories4 = sparkCreateModel.Tag4;
            sparkModel.FKCategories5 = sparkCreateModel.Tag5;

            sparkModel.FKAccountsCreatedBy = nQryUserId.First();
            sparkModel.dDateCreated = DateTime.Now;
            sparkModel.dDateModified = DateTime.Now;
            m_db.sparks.Add(sparkModel);
            if (SaveChanges())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Method to get all arguments for a given spark.
        /// </summary>
        /// <param name="nSparkId">Id of spark</param>
        /// <returns>List of arguments</returns>
        public static List<Models.arguments> GetAllArgumentsForSpark(int nSparkId)
        {
            List<arguments> lstArguments = new List<arguments>();

            var arguments = from r in m_db.arguments
                            where r.FKSparks == nSparkId
                            select r;

            foreach(arguments argument in arguments)
            {
                lstArguments.Add(argument);
            }

            return lstArguments;
        }

        /// <summary>
        /// Adds an argument to the database
        /// </summary>
        /// <param name="argumentModel">argument to add</param>
        /// <returns>Success</returns>
        public static bool CreateArgument(arguments argumentModel)
        {
            m_db.arguments.Add(argumentModel);
            return SaveChanges();
        }
    }
}