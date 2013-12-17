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
            
            try
            {
                m_db.sparks.Add(sparkModel);
                m_db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string str1 = validationError.PropertyName;
                        string str2 = validationError.ErrorMessage;
                    }
                }
                // log ex.
                return false;
            }

            return true;
        }
    }
}