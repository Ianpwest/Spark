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
            sparkModel.strTitle = sparkCreateModel.Title;
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

            sparkModel.FKAccountsCreatedBy = nQryUserId.First();
            sparkModel.dDateCreated = DateTime.Now;
            sparkModel.dDateModified = DateTime.Now;
            db.sparks.Add(sparkModel);

            
            if (SaveChanges(db) && AddBaselineInterest(sparkModel.PK, nQryUserId.First()))
            {
                return sparkModel.PK;
            }
                
            else
                return int.MinValue;
        }

        private static bool AddBaselineInterest(int nFKSpark, int nUserID)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            //Give baseline interest of one upvote from the creator.
            sparkinterestvotes siv = new sparkinterestvotes();
            siv.bIsDeleted = false;
            siv.bIsUpVote = true;
            siv.FKSparks = nFKSpark;
            siv.FKAccounts = nUserID;

            db.sparkinterestvotes.Add(siv);

            if (SaveChanges(db))
                return true;

            return false;
        }

        /// <summary>
        /// Method to get all arguments for a given spark.
        /// </summary>
        /// <param name="nSparkId">Id of spark</param>
        /// <returns>List of arguments</returns>
        public static List<Models.arguments> GetAllArgumentsForSpark(int nSparkId)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            return Calculations.SortArgumentsByPopularity(db, nSparkId); ;
        }

        /// <summary>
        /// Adds an argument to the database
        /// </summary>
        /// <param name="argumentModel">argument to add</param>
        /// <returns>Success</returns>
        public static bool CreateArgument(arguments argumentModel)
        {
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            bool bSuccess = false;

            try
            {
                argumentModel.dDateCreated = DateTime.Now;
                argumentModel.dDateCreated = DateTime.Now;
                db.arguments.Add(argumentModel);
                bSuccess =  SaveChanges(db);
            }
            catch
            {
                LogError("System", "Failed to create argument");
                bSuccess = false;
            }

            return bSuccess;
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
                List<KeyValuePair<int, bool>> lstVotes = new List<KeyValuePair<int, bool>>();
                foreach (argumentvotes vote in qryVotesUsers)
                {
                    lstVotes.Add(new KeyValuePair<int, bool>(vote.FKAccounts, vote.bIsUpvote));
                }

                foreach(KeyValuePair<int, bool> kvp in lstVotes)
                {
                    if (kvp.Value)
                        dblInfluenceScore += Calculations.Influence(db, kvp.Key, nCategoryId);
                    else
                        dblInfluenceScore -= Calculations.Influence(db, kvp.Key, nCategoryId);
                }
            }

            return dblInfluenceScore;
        }

        #endregion

        /// <summary>
        /// Determines the user's voting status concerning a given spark and given userId. Status 0 = no user vote, 1 = user upvote, 2 = user downvote.
        /// </summary>
        /// <param name="nUserId">User Id to check for a vote.</param>
        /// <param name="nSparkId">Spark Id to check for a vote.</param>
        /// <returns>Returns 1 for a user upvote, 2 for a user downvote, else returns 0.</returns>
        public static int GetUserSparkVoteStatus(int nUserId, int nSparkId)
        {
            sparkdbEntities1 db = GetDatabaseInstance();

            var qry = from r in db.sparkinterestvotes
                      where r.FKAccounts == nUserId && r.FKSparks == nSparkId && r.bIsDeleted == false
                      select r;

            if (qry != null && qry.Count() > 0)
            {
                if (qry.First().bIsUpVote)
                    return 1; // User has upvoted the spark
                else
                    return 2; // User has downvoted the spark
            }
            else
                return 0; // User has not voted on the spark
        }

        public static int CastSparkVote(int nSparkId, bool bIsUpvote, string strUserName)
        {
            int nStatus = 0;
            sparkdbEntities1 db = GetDatabaseInstance();

            int nUserId = GetUserId(db, strUserName);
            if (nUserId == int.MinValue)
                return -1; // failure

            sparks sparkExisting = GetExistingSpark(db, nSparkId); // Checks if a vote already exists for this user on this spark.
            if (sparkExisting == null)
                return -1; // failure

            sparkinterestvotes voteExisting = GetExistingSparkVote(db, nUserId, nSparkId);
            if (voteExisting == null) // NEW VOTE 
            {
                sparkinterestvotes vote = new sparkinterestvotes();
                vote.FKAccounts = nUserId;
                vote.FKSparks = nSparkId;
                vote.bIsUpVote = bIsUpvote;
                vote.bIsDeleted = false;
                db.sparkinterestvotes.Add(vote);
            }
            else if (voteExisting.bIsDeleted == true) // VOTE EXISTS BUT IS DELETED
            {
                voteExisting.bIsUpVote = bIsUpvote;
                voteExisting.bIsDeleted = false;
            }
            else
            {
                if (voteExisting.bIsUpVote == bIsUpvote) // UNVOTE
                {
                    // Undo the vote if the user attempts to re-enter the same vote by deleting it.
                    voteExisting.bIsDeleted = true;
                    nStatus = 3; // indicating that the vote is being removed.
                }
                else // CHANGE VOTE
                {
                    voteExisting.bIsUpVote = bIsUpvote;
                    voteExisting.bIsDeleted = false;
                    nStatus = 1; // Indicates that the vote already exists but is being changed.
                }
            }

            if (SaveChanges(db))
                return nStatus; //return either 0 for new vote or 1 for changed vote.
            else
                return -1; // return failure status.
        }

        public static int GetSparkVoteCount(int nSparkId, bool bIsUpvote)
        {
            sparkdbEntities1 db = GetDatabaseInstance();
            int nCount = 0;

            var qryVoteCount = from r in db.sparkinterestvotes
                               where r.FKSparks == nSparkId && r.bIsUpVote == bIsUpvote && r.bIsDeleted == false
                               select r;

            if (qryVoteCount != null && qryVoteCount.Count() > 0)
                nCount = qryVoteCount.Count();

            return nCount;
        }

        public static List<sparks> GetSparksForUser(string strUserName, int nIndex, int nCount)
        {
            List<sparks> lstReturn = new List<sparks>();
            List<sparks> lstAllSparks = new List<sparks>();
            sparkdbEntities1 db = GetDatabaseInstance();
            int nUserID = GetUserId(db,strUserName);
            var sparks = from r in db.sparks
                         where r.FKAccountsCreatedBy == nUserID
                         orderby r.dDateCreated
                         select r;

            
            foreach(sparks spark in sparks)
            {
                lstAllSparks.Add(spark);
            }

            for(int i = nIndex; i <= nCount; i++)
            {
                if (lstAllSparks.Count == 0 || lstAllSparks.Count <= i)
                    break;
                lstReturn.Add(lstAllSparks[i]);
            }

            return lstReturn;
        }

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

            tags cat = new tags();
            cat.strName = strName;
            cat.strImageName = strImgName;

            db.tags.Add(cat);
            if (!SaveChanges(db))
                return new KeyValuePair<int,string>(int.MinValue, string.Empty);

            return new KeyValuePair<int,string>(cat.PK, cat.strName);
        }

        public static int CastArgumentVote(int nArgumentId, bool bIsUpvote, string strUserName)
        {
            int nStatus = 0;
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            int nUserId = GetUserId(db, strUserName);
            if (nUserId == int.MinValue)
                return -1; // failure

            // Verify Argument exists 
            arguments argExisting = GetExistingArgument(db, nArgumentId);
            if(argExisting == null)
                return -1; // failure

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

                if (voteExisting.bIsUpvote == bIsUpvote)
                {
                    // Undo the vote if the user attempts to re-enter the same vote by deleting it.
                    db.argumentvotes.Remove(voteExisting);
                    nStatus = 3; // indicating that the vote is being removed.
                }
                else
                {
                    voteExisting.bIsUpvote = bIsUpvote;
                    nStatus = 1; // Indicates that the vote already exists and simply switched.
                }
            }

            if (SaveChanges(db))
                return nStatus; //return either 0 for new vote or 1 for changed vote.
            else
                return -1; // return failure status.
        }

        private static sparks GetExistingSpark(sparkdbEntities1 db, int nSparkId)
        {
            var qrySpark = from r in db.sparks
                               where r.PK == nSparkId
                               select r;
            if (qrySpark == null || qrySpark.Count() != 1)
            {
                LogNonUserError("Unable to find spark id = " + nSparkId + " in the database.", "", "", "SparkDatabaseInterface", "GetExistingSpark", "qrySpark");
                return null;
            }

            return qrySpark.First();
        }

        private static sparkinterestvotes GetExistingSparkVote(sparkdbEntities1 db, int nUserId, int nSparkId)
        {
            var qryExistingVote = from r in db.sparkinterestvotes
                                  where r.FKAccounts == nUserId && r.FKSparks == nSparkId
                                  select r;

            if (qryExistingVote == null || qryExistingVote.Count() < 1)
                return null; // Returning a code indicating the user has no vote yet.

            return qryExistingVote.First();
        }

        private static arguments GetExistingArgument(sparkdbEntities1 db, int nArgumentId)
        {
            var qryArguments = from r in db.arguments
                               where r.PK == nArgumentId
                               select r;
            if (qryArguments == null || qryArguments.Count() != 1)
            {
                LogNonUserError("Unable to find argument id = " + nArgumentId + " in the database.", "", "", "SparkDatabaseInterface", "GetExistingArgument", "qryArguments");
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

        /// <summary>
        /// Get the users previous vote (if any) for the given argument.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static bool? GetUserVoteForArgument(arguments argument)
        {
            sparkdbEntities1 db = GetDatabaseInstance();

            var vote = from r in db.argumentvotes
                       where r.FKAccounts == argument.FKAccounts
                       && r.FKArguments == argument.PK
                       select r.bIsUpvote;

            if (vote.Count() == 0)
                return null;

            return vote.FirstOrDefault();
        }

        /// <summary>
        /// Method to return the list of categories in the database. Name only.
        /// </summary>
        /// <returns>Category names in a list</returns>
        public static Dictionary<int, string> GetAllCategories()
        {
            Dictionary<int, string> lstReturn = new Dictionary<int, string>();
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            var lstSubjectMatters = from r in db.subjectmatters
                                    orderby r.strName
                                    select r;

            if (lstSubjectMatters == null || lstSubjectMatters.Count() <= 0)
                return null;

            foreach(var x in lstSubjectMatters)
            {
                lstReturn.Add(x.PK, x.strName);
            }

            return lstReturn;
        }

        /// <summary>
        /// Method to return the list of tags in the database. Name only.
        /// </summary>
        /// <returns>Tag names in a list</returns>
        public static Dictionary<int, string> GetAllTags()
        {
            Dictionary<int, string> lstReturn = new Dictionary<int, string>();
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            var lstTags = from r in db.tags
                                where r.PK >= 0
                                orderby r.strName
                                select r;

            if (lstTags == null || lstTags.Count() <= 0)
                return lstReturn;

            foreach (var x in lstTags)
            {
                lstReturn.Add(x.PK, x.strName);
            }

            return lstReturn;
        }

        /// <summary>
        /// Returns a List of categories objects that contain information about the requested tags.
        /// The tags are requested by a List of integers representing the database primary keys.
        /// </summary>
        /// <param name="lstPKs"></param>
        /// <returns></returns>
        public static List<tags> GetTagFileName(List<int> lstPKs)
        {
            List<tags> lstTags = new List<tags>();
            sparkdbEntities1 db = BaseDatabaseInterface.GetDatabaseInstance();

            var qryTags = from r in db.tags
                          where lstPKs.Contains(r.PK)
                          select r;

            if (qryTags == null || qryTags.Count() < 1)
                return lstTags;

            foreach (tags tag in qryTags)
            {
                lstTags.Add(tag);
            }

            return lstTags;
        }
      
    }
}