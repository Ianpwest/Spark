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
        public static bool CreateSpark(SparkCreateModel sparkCreateModel)
        {
            sparks sparkModel = new sparks();
            sparkModel.strDescription = sparkCreateModel.Description;
            sparkModel.strTopic = sparkCreateModel.Topic;
            
            sparkModel.FKSubjectMatters = sparkCreateModel.SubjectMatterId;

            var nQryUserId = from r in m_db.accounts
                             join p in m_db.profiles on r.PK equals p.FKAccounts
                             where r.strUserName == sparkCreateModel.UserId
                             select p.PK;

            if (nQryUserId == null || nQryUserId.Count() != 1)
                return false;

            sparkModel.FKProfilesCreatedBy = nQryUserId.First();
            sparkModel.dDateCreated = DateTime.Now;
            sparkModel.dDateModified = DateTime.Now;

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