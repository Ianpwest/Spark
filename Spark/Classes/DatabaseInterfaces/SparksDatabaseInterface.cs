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
        public static int CreateSpark(SparkCreateModel sparkCreateModel)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            sparks sparkModel = new sparks();
            sparkModel.strDescription = sparkCreateModel.Description;
            sparkModel.strTopic = sparkCreateModel.Topic;
            
            sparkModel.FKSubjectMatters = sparkCreateModel.SubjectMatterId;
            
            var nQryUserId = from r in db.accounts
                             where r.strUserName == sparkCreateModel.UserId
                             select r.PK;

            if (nQryUserId == null || nQryUserId.Count() != 1)
                return int.MinValue;

            sparkModel.FKCategories1 = sparkCreateModel.Tag1;
            sparkModel.FKCategories2 = sparkCreateModel.Tag2;
            sparkModel.FKCategories3 = sparkCreateModel.Tag3;
            sparkModel.FKCategories4 = sparkCreateModel.Tag4;
            sparkModel.FKCategories5 = sparkCreateModel.Tag5;

            sparkModel.FKAccountsCreatedBy = nQryUserId.First();
            sparkModel.dDateCreated = DateTime.Now;
            sparkModel.dDateModified = DateTime.Now;
            db.sparks.Add(sparkModel);
            if (SaveChanges(db))
                return sparkModel.PK;
            else
                return int.MinValue;
        }

        /// <summary>
        /// Method to get all arguments for a given spark.
        /// </summary>
        /// <param name="nSparkId">Id of spark</param>
        /// <returns>List of arguments</returns>
        public static List<Models.arguments> GetAllArgumentsForSpark(int nSparkId)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            List<arguments> lstArguments = new List<arguments>();

            var arguments = from r in db.arguments
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
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            
            db.arguments.Add(argumentModel);
            return SaveChanges(db);
        }

        /// <summary>
        /// Get the spark given the spark id    
        /// </summary>
        /// <param name="nSparkId">spark id</param>
        /// <returns>spark</returns>
        public static sparks GetSpark(int nSparkId)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            return (from r in db.sparks
                    where r.PK == nSparkId
                    select r).FirstOrDefault();
        }

        /// <summary>
        /// Get the argument given the id   
        /// </summary>
        /// <param name="id">argument id</param>
        /// <returns>argument</returns>
        public static arguments GetArgument(int id)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            return (from r in db.arguments
                    where r.PK == id
                    select r).FirstOrDefault();
        }

        public static SparkArgumentModel BuildSparkArgumentModel(arguments argument)
        {
            sparkdbEntities1 db = GetDatabaseInstance();
            //Need to pass this argument to the analytics engine to fill in the rest of the fields?
            SparkArgumentModel sam = new SparkArgumentModel();

            sam.id = argument.PK;
            sam.bIsAgree = argument.bIsAgree;
            sam.nArgumentScore = GetArgumentScore(argument.PK, db);
            sam.nCommentCount = GetArgumentCommentCount(argument.PK, db);
            sam.nDownVote = GetArgumentVoteCount(argument.PK, db, false);
            sam.nInfluenceScore = GetArgumentInfluenceCount(argument.PK, db);
            sam.nUpVote = GetArgumentVoteCount(argument.PK, db, true); 
            sam.strArgument = argument.strArgument;
            sam.strCitations = argument.strCitations;
            sam.strConclusion = argument.strConclusion;
            sam.strUserName = AccountsDatabaseInterface.GetUsername(argument.FKAccounts);

            return sam;
        }

        #region Spark Argument Analysis

        private static int GetArgumentScore(int argId, sparkdbEntities1 db)
        {
            //this should come from the analytics result. Haven't decided what this should be really.
            return 25;
        }

        private static int GetArgumentCommentCount(int argId, sparkdbEntities1 db)
        {
            int nCommentCount = 0;

            var qryArgumentVotes = from r in db.comments
                                   where r.FKArguments == argId
                                   select r;

            if (qryArgumentVotes != null)
                nCommentCount = qryArgumentVotes.Count();

            return nCommentCount;
        }

        private static int GetArgumentVoteCount(int argId, sparkdbEntities1 db, bool bIsUpVote)
        {
            int nUpVoteCount = 0;

            var qryUpVotes = from r in db.argumentvotes
                             where r.FKArguments == argId && r.bIsUpvote == bIsUpVote
                             select r;

            if (qryUpVotes != null)
                nUpVoteCount = qryUpVotes.Count();

            return nUpVoteCount;
        }

        private static double GetArgumentInfluenceCount(int argId, sparkdbEntities1 db)
        {
            double dblInfluenceScore = 0;
            int nCategoryId = int.MinValue;

            var qryVotesUsers = from r in db.argumentvotes
                           where r.FKArguments == argId
                           select r;

            var qrySparkId = from r in db.arguments
                                      where r.PK == argId
                                      select r.FKSparks;

            if(qrySparkId != null && qrySparkId.Count() > 0)
            {
                int nSparkId = qrySparkId.First();

                var qryCategory = from r in db.sparks
                                  where r.PK == nSparkId
                                  select r.FKSubjectMatters;

                if(qryCategory != null && qryCategory.Count() > 0)
                    nCategoryId = qryCategory.First();
                else
                {
                    // Log error here and maybe alert user?
                    return 0;
                }
            }

            if(qryVotesUsers != null)
            {
                foreach(argumentvotes vote in qryVotesUsers)
                {
                    if (vote.bIsUpvote)
                        dblInfluenceScore += Calculations.Influence(vote.FKAccounts, nCategoryId);
                    else
                        dblInfluenceScore -= Calculations.Influence(vote.FKAccounts, nCategoryId);
                }
            }

            return dblInfluenceScore;
        }

        #endregion

        /// <summary>
        /// Uploads a tag to the database and returns a keyvaluepair<int,string> : key = PK, value = strName.
        /// Returns int.MinValue, string.empty if unsuccessful in the upload.
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strImgName"></param>
        /// <returns></returns>
        public static KeyValuePair<int,string> UploadTag(string strName, string strImgName)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            categories cat = new categories();
            cat.strName = strName;
            cat.strImageName = strImgName;

            db.categories.Add(cat);
            if (!SaveChanges(db))
                return new KeyValuePair<int,string>(int.MinValue, string.Empty);

            return new KeyValuePair<int,string>(cat.PK, cat.strName);
        }

        public static bool UploadArgumentData(int nArgumentId, bool bIsUpvote, string strUserName)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            int nUserId = GetUserId(db, strUserName);
            if (nUserId == int.MinValue)
                return false;

            // Verify Argument exists 
            arguments argExisting = GetExistingArgument(db, nArgumentId);
            if(argExisting == null)
                return false;

            argumentvotes voteExisting = GetExistingArgumentVote(db, nUserId, nArgumentId);
            // TODO - fix this later, needs to change the arg instead of make new one if it already exists.
            if(voteExisting == null)
            { 
                argumentvotes vote = new argumentvotes();
                vote.FKAccounts = nUserId;
                vote.FKArguments = nArgumentId;
                vote.bIsUpvote = bIsUpvote;

                db.argumentvotes.Add(vote);
            }
            else
            {
                if(voteExisting.bIsUpvote && bIsUpvote)
                    return false;  
                else
                {
                    voteExisting.bIsUpvote = bIsUpvote;
                }
            }
            
            return SaveChanges(db);
        }

        private static arguments GetExistingArgument(sparkdbEntities1 db, int nArgumentId)
        {
            var qryArguments = from r in db.arguments
                               where r.PK == nArgumentId
                               select r;
            if (qryArguments == null || qryArguments.Count() != 1)
            {
                LogNonUserError("Unable to find argument id = " + nArgumentId + " in the database.", "", "", "SparkDatabaseInterface", "UploadArgumentData", "qryArguments");
                return null;
            }

            return qryArguments.First();
        }

        private static argumentvotes GetExistingArgumentVote(sparkdbEntities1 db, int nUserId, int nArgumentId)
        {
            var qryExistingVote = from r in db.argumentvotes
                                  where r.FKAccounts == nUserId && r.FKArguments == nArgumentId
                                  select r;

            if (qryExistingVote == null || qryExistingVote.Count() < 1) 
                return null; // Returning a code indicating the user has no vote yet.

            return qryExistingVote.First();
        }
    }
}