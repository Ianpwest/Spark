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
            sparkModel.strImagePath = sparkCreateModel.FileName;
            
            sparkModel.FKSubjectMatters = sparkCreateModel.SubjectMatterId;
            
            var nQryUserId = from r in m_db.accounts
                             where r.strUserName == sparkCreateModel.UserId
                             select r.PK;

            if (nQryUserId == null || nQryUserId.Count() != 1)
                return false;

            sparkModel.FKAccountsCreatedBy = nQryUserId.First();
            sparkModel.dDateCreated = DateTime.Now;
            sparkModel.dDateModified = DateTime.Now;
            m_db.sparks.Add(sparkModel);
            if (SaveChanges())
                return true;
            else
                return false;
        }
    }
}