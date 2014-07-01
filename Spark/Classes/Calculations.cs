using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Models;

namespace Spark.Classes
{
    /// <summary>
    /// Static class used to perform various algorithmic calculations for any data that is not directly stored in the database.
    /// </summary>
    public static class Calculations
    {
        #region Influence Calculations

        /// <summary>
        /// Used to calculate a particular user's total influence.
        /// </summary>
        /// <returns></returns>
        public static double Influence(sparkdbEntities1 db, int nUserId, int nSubjectMatterId)
        {
            double dblInfluence = 0;

            dblInfluence += GetBaseInfluence(db, nUserId, nSubjectMatterId);
            dblInfluence += GetUserAddons(db, nUserId, nSubjectMatterId);
            dblInfluence += GetSpread(db, nUserId, nSubjectMatterId);

            return dblInfluence;
        }

        /// <summary>
        /// Calculates base influence for a specific user in a specific broad category. This influence is base and is not affected by any other influencial gains.
        /// </summary>
        /// <param name="userCurrent">Account Id for user.</param>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nSubjectMatterId">Broad category against which to calculate.</param>
        /// <returns>Double value that represents the base influence number.</returns>
        private static double GetBaseInfluence(sparkdbEntities1 db, int nUserId, int nSubjectMatterId)
        {
            double dblBase = 0;

            double dblGains = (from influencegains influencegain in db.influencegains
                               where influencegain.FKAccountsReceived == nUserId && influencegain.bIsPositive == true && influencegain.FKSubjectMatters == nSubjectMatterId
                               select influencegain).Count();

            double dblLosses = (from influencegains influencegain in db.influencegains
                                where influencegain.FKAccountsReceived == nUserId && influencegain.bIsPositive == false && influencegain.FKSubjectMatters == nSubjectMatterId
                                select influencegain).Count();

            // Returns the number of positive votes minus the number of negative votes from the gains table to get the absolute scale.
            dblBase = dblGains - dblLosses;

            double nConstant = 1;
            if (double.TryParse((from constants constant in db.constants
                                where constant.strKey == "InfluenceBaseConstant"
                                select constant.strValue).FirstOrDefault(), out nConstant))
                dblBase = dblBase * nConstant;

            return dblBase;
        }

        /// <summary>
        /// Calculates the additional gains given by all other user's base influence effect on the given user.
        /// </summary>
        /// <param name="userCurrent">Account Id for user.</param>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nSubjectMatterId">Broad category against which to calculate.</param>
        /// <returns>Double value that represents a total of all related user gained/lost influence.</returns>
        private static double GetUserAddons(sparkdbEntities1 db, int nUserId, int nSubjectMatterId)
        {
            double dblUserAddon = 0;

            // Queries for all of the userIds of people who contributed to the initial user's influence in the gains table.
            IEnumerable<int> qryAllContributors = from influencegains influence in db.influencegains
                                                  where influence.FKAccountsReceived == nUserId
                                                  select influence.FKAccountsReceived;
            
            // Attempts to determine the constant to use for userAddon values.
            double nConstant = 1;
            double.TryParse((from constants constant in db.constants
                            where constant.strKey == "InfluenceUserAddonConstant"
                            select constant.strValue).FirstOrDefault(), out nConstant);

            List<int> lstContributors = new List<int>(); // Creating memory allocation of query to close the reader before executing another query. **
            foreach(int n in qryAllContributors)
            {
                lstContributors.Add(n);
            }

            foreach (int n in lstContributors)
            {
                // Finds the base influence value for each of the users who contributed to the current user's influence.
                // Multiplies the base value found by a constant, then adds it to the running total.
                dblUserAddon += (GetBaseInfluence(db, n, nSubjectMatterId) * nConstant);
            }
            
            return dblUserAddon;
        }

        /// <summary>
        /// Calculates the total amount of spread influence the user gains for a specified broad category given their influence in related broad categories.
        /// </summary>
        /// <param name="userCurrent">Account Id for user.</param>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nSubjectMatterId">Broad category against which to calculate.</param>
        /// <returns>Double value that represents a sum of all spread gains/losses for the given broad category.</returns>
        private static double GetSpread(sparkdbEntities1 db, int nUserId, int nSubjectMatterId)
        {
            double dblSpread = 0;

            IEnumerable<int> qrySpreadValues = from subjectmatterspreads spread in db.subjectmatterspreads
                                               where spread.FKSubjectMattersSpread == nSubjectMatterId &&
                                               spread.FKSubjectMattersContributor == nUserId
                                               select spread.nValue;
            foreach (int n in qrySpreadValues)
            {
                dblSpread += n;
            }

            return dblSpread;
        }

        #endregion

        #region Spark Sorting

        /// <summary>
        /// Provides a list of sparks sorted by their popularity. Popularity is based on spark interest votes.
        /// </summary>
        /// <param name="dbEntity">Database entity.</param>
        /// <returns>Sorted list of sparks.</returns>
        public static List<sparks> SortSparksByPopularity(sparkdbEntities1 dbEntity)
        {
            List<sparks> lstSorted = new List<sparks>();
            Dictionary<sparks, double> dictSparkValues = ApplyPopularitySorting(dbEntity);

            Dictionary<sparks, double> dictDecayed = ApplyDecayAlgorithm(dictSparkValues, dbEntity);

            // Sorts the dictionary by the double values in descending order to get the highest value spark
            // at the first index and converts it to a list.
            lstSorted = (from KeyValuePair<sparks, double> kvp in dictDecayed
                         orderby kvp.Value descending
                         select kvp.Key).ToList<sparks>();

            return lstSorted;
        }

        /// <summary>
        /// Provides a list of sparks sorted by their popularity. This collection contains sparks started at the desired index
        /// </summary>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nIndex">Index for which to begin collecting sparks.</param>
        /// <param name="nCount">Number of sparks to collect.</param>
        /// <returns>Sorted list of sparks starting at the specified index and containing at most the number of sparks specified.</returns>
        public static List<sparks> GetSparksByPopularityRange(sparkdbEntities1 dbEntity, int nIndex, int nCount)
        {
            List<sparks> lstReturn = new List<sparks>();
            List<sparks> lstSorted = SortSparksByPopularity(dbEntity);

            if (nIndex >= lstSorted.Count) // if the index does not exist in the collection, return an empty collection.
                return lstReturn;

            for (int i = nIndex; i < nIndex + nCount; i++) // adds each sorted element to the return list
            {
                if (i >= lstSorted.Count) // Break out of the loop if we are attempting to access indexes that exceed the collection
                    break;

                lstReturn.Add(lstSorted[i]);
            }

            return lstReturn;
        }

        /// <summary>
        /// Provies a list of sparks sorted by their popularity. This collection contains sparks whose primary keys do not exist in the
        /// parameter collection of keys. The list returned will not exceed a count of the specified count parameter.
        /// </summary>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nCount">Number of sparks to collect.</param>
        /// <param name="lstCurrent">Collection of spark PKs which to skip over while collecting the sorted list.</param>
        /// <returns>Sorted list of sparks whose primary keys are not contained in the parameter collection and whose count does not exceed the given parameter.</returns>
        public static List<sparks> GetNextSetSparks(sparkdbEntities1 dbEntity, int nCount, List<int> lstCurrent)
        {
            List<sparks> lstReturn = new List<sparks>();
            List<sparks> lstSorted = SortSparksByPopularity(dbEntity);

            foreach (sparks spark in lstSorted)
            {
                if (lstReturn.Count >= nCount) // only get as many sparks as was requested
                    break;

                if (lstCurrent.Contains(spark.PK)) // skip any sparks that are given in the current sparks list
                    continue;

                lstReturn.Add(spark);
            }
            
            return lstReturn;
        }

        /// <summary>
        /// Provides a list of sparks sorted by broad category given the broad category Id.
        /// </summary>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nSubjectMatterId"></param>
        /// <returns></returns>
        public static List<sparks> SortSparksBySubject(sparkdbEntities1 dbEntity, int nSubjectMatterId)
        {
            List<sparks> lstSorted = new List<sparks>();

            Dictionary<sparks, double> dictPopularitySorted = ApplyPopularitySorting(dbEntity); // collects all of the sparks with popularity values

            // Gets a collection of the sparks that belong to a specific broad category.
            var qrySparksBySubject = from sparks spark in dbEntity.sparks
                                     where spark.FKSubjectMatters == nSubjectMatterId
                                     select spark;
            
            // Iterates through the popularity sorted dictionary and sorts the return collection by the value.
            var qrySortPopulartyColl = from KeyValuePair<sparks, double> kvp in dictPopularitySorted
                                       orderby kvp.Value descending
                                       select kvp;

            Dictionary<sparks, double> dictSubjectPopSorted = new Dictionary<sparks, double>();

            // Checks the popularity list and adds all of the sparks to the return list if they exist in the subject list.
            foreach (KeyValuePair<sparks, double> kvp in qrySortPopulartyColl)
            {
                if(dictSubjectPopSorted.ContainsKey(kvp.Key))
                    continue;

                if (qrySparksBySubject.Contains(kvp.Key))
                    dictSubjectPopSorted.Add(kvp.Key, kvp.Value);
            }

            // Adds all of the remaining sparks from the subject list if they have not been added already.
            foreach (sparks spark in qrySparksBySubject)
            {
                if (!dictSubjectPopSorted.ContainsKey(spark))
                    dictSubjectPopSorted.Add(spark, 0);
            }

            lstSorted = (from KeyValuePair<sparks, double> kvp in ApplyDecayAlgorithm(dictSubjectPopSorted, dbEntity)
                         orderby kvp.Value descending
                         select kvp.Key).ToList<sparks>();

            return lstSorted;
        }

        /// <summary>
        /// Returns a collection of sparks that match the input parameter filters. Sorts the sparks by popularity then filters out all sparks that do not 
        /// match the parameters. Use int.MinValue for nCategory and nTag to not use these filters. Use string.empty to not use the strSearch filter.
        /// Returns a simple list of all sparks sorted by populary if no parameters are used.
        /// </summary>
        /// <param name="db">Database instance to use to sort and filter sparks.</param>
        /// <param name="nCategory">Selected category - only returns sparks that are in this category.</param>
        /// <param name="nTag">Selected tag - only return sparks that are associated with this tag.</param>
        /// <param name="strSearch">Selected search string - only returns sparks that contain this string in their topic field.</param>
        /// <returns>Returns a list of sparks that are sorted by popularity and filtered by the input parameters.</returns>
        public static List<sparks> FilterSparksByHomeParameters(sparkdbEntities1 db, int nCategory, int nTag, string strSearch)
        {
            List<sparks> lstFiltered = new List<sparks>();

            lstFiltered = SortSparksByPopularity(db); // Gets the generic collection of sorted sparks.

            // Query designed to find all sparks that are not the selected category.
            var qryCategories = from r in lstFiltered
                                where r.FKSubjectMatters != nCategory
                                select r;

            // Query designed to find all sparks that do not associate with the selected tag.
            var qryTag = from r in lstFiltered
                         where r.FKCategories1 != nTag && r.FKCategories2 != nTag && r.FKCategories3 != nTag &&
                         r.FKCategories4 != nTag && r.FKCategories5 != nTag
                         select r;

            // Query designed to find all sparks that do not contain the selected search string in the topic.
            var qrySearchString = from r in lstFiltered
                                  where !r.strTopic.Contains(strSearch)
                                  select r;

            List<sparks> lstRemovals = new List<sparks>(); // Creates a set that will show which pieces to remove from the base collection.
            
            if (nCategory != int.MinValue) // int.minvalue will be used to turn off this sort piece.
            {
                foreach(sparks spark in qryCategories) // goes through the categories filter to determine which pieces to remove
                {
                    if (!lstRemovals.Contains(spark))
                        lstRemovals.Add(spark);
                }
            }

            if (nTag != int.MinValue) // int.minvalue will be used to turn off this sort piece.
            {
                foreach (sparks spark in qryTag)
                {
                    if (!lstRemovals.Contains(spark)) // goes through the tag filter to determine which pieces to remove
                        lstRemovals.Add(spark);
                }
            }

            if (!string.IsNullOrEmpty(strSearch)) // int.minvalue will be used to turn off this sort piece.
            {
                foreach (sparks spark in qrySearchString) // goes through the search string filter to determine which pieces to remove
                {
                    if (!lstRemovals.Contains(spark))
                        lstRemovals.Add(spark);
                }
            }

            foreach (sparks spark in lstRemovals) // From all of the selected sparks from the base filtered collection.
            {
                if (lstFiltered.Contains(spark))
                    lstFiltered.Remove(spark);
            }

            return lstFiltered;
        }

        /// <summary>
        /// Returns a complete list of sparks from the database sorted by their created date.
        /// Setting the boolean parameter to true will sort by descending, else it will sort by ascending.
        /// </summary>
        /// <param name="db">Database instance in which to sort spark records.</param>
        /// <param name="bIsDescending">True for descending sorting, else ascending sorting.</param>
        /// <returns>Returns a collection of sparks sorted by their created date.</returns>
        public static List<sparks> SortSparksByDate(sparkdbEntities1 db, bool bIsDescending)
        {
            List<sparks> lstSorted = new List<sparks>();

            if (bIsDescending)
            {
                var qrySparksDescending = from r in db.sparks
                                          orderby r.dDateCreated descending
                                          select r;

                if (qrySparksDescending != null)
                {
                    foreach (sparks spark in qrySparksDescending)
                    {
                        if (!lstSorted.Contains(spark))
                            lstSorted.Add(spark);
                    }
                }
            }
            else
            {
                var qrySparksAscending = from r in db.sparks
                                         orderby r.dDateCreated ascending
                                         select r;
                if (qrySparksAscending != null)
                {
                    foreach (sparks spark in qrySparksAscending)
                    {
                        if (!lstSorted.Contains(spark))
                            lstSorted.Add(spark);
                    }
                }
            }

            return lstSorted;
        }

        /// <summary>
        /// Returns a list of sparks that are associated with any of the tag ids in the parameter list.
        /// </summary>
        /// <param name="db">Database instance to search for sparks.</param>
        /// <param name="lstTags">List of tag IDs which is used to check against a spark's associated tags.</param>
        /// <returns>Returns a list of sparks that are associated with the parameterized list of tags ids.</returns>
        public static List<sparks> FilterSparksByMultipleTags(sparkdbEntities1 db, List<int> lstTags)
        {
            List<sparks> lstFiltered = new List<sparks>();

            // Queries to pull back all sparks where one of the tags in the list passed in is associated with the spark's tag.
            var qrySparks = from r in db.sparks
                            where lstTags.Contains(r.FKCategories1) || lstTags.Contains(r.FKCategories2) || lstTags.Contains(r.FKCategories3) ||
                            lstTags.Contains(r.FKCategories4) || lstTags.Contains(r.FKCategories5)
                            select r;

            if (qrySparks != null)
            {
                foreach (sparks spark in qrySparks)
                {
                    if(!lstFiltered.Contains(spark))
                        lstFiltered.Add(spark);
                }
            }

            return lstFiltered;
        }

        /// <summary>
        /// Applies the decaying algorithm to a sorted dictionary of sparks keyed by the spark with values of the sort position value.
        /// The algorithm has growth at 0 hours and begins to decay once (hours / 24 ) equals the database constant "SortingDecayShifter"
        /// Decays the values based on the date that the spark was created. Uses constants from the constants table to tune decay start and sensitivity.
        /// Increases in SortingDecayMultiplier (C) value will cause the created date to influence the sort more dominantly instead of the spark vote.
        /// Increases in the SortingDecayShifter (k) value will cause the decay to take more hours/days to make a significant effect on the sort.
        /// </summary>
        /// <param name="dictSparks">Dictionary keyed by spark and values of the sorting determined by the popularity method or some other method for determining sort values.</param>
        /// <param name="dbEntity">Database instance in which to collect constant values for the decay algorithm.</param>
        /// <returns>Returns the same collection as passed in as a parameter with modified values from the decay algorithm.</returns>
        private static Dictionary<sparks, double> ApplyDecayAlgorithm(Dictionary<sparks, double> dictSparks, sparkdbEntities1 dbEntity)
        {
            Dictionary<sparks, double> dictSparkValues = new Dictionary<sparks, double>();

            // Creating decay function that will give negative values to the spark depending on how long it has been since created.
            // P = C(1 - e^(x-k))  where C is a multiplier constant and k is a shifter constant.

            double dblConstantMultiplier = 1;
            double dblConstantShifter = 0;

            // This constant is the sensitive to the decay. C should not be negative or else we will see exponential growth instead of decay.
            // Small values (Between 0 and 1) will put less emphasis on date created and more emphasis on spark votes, and vice versa.
            double.TryParse((from constants constant in dbEntity.constants
                             where constant.strKey == "SortingDecayMultiplier" // C in the equation
                             select constant.strValue).FirstOrDefault(), out dblConstantMultiplier);
            // This constant is how we determine when we want the decay to start. When x = k, the decay is at 0%.
            // For values of x < k, we have negative decay (growth), and the smaller or more negative k is, the earlier and stronger the decay is.
            double.TryParse((from constants constant in dbEntity.constants
                             where constant.strKey == "SortingDecayShifter" // k in the equation
                             select constant.strValue).FirstOrDefault(), out dblConstantShifter);

            foreach (KeyValuePair<sparks, double> kvp in dictSparks)
            {
                if (dictSparkValues.ContainsKey(kvp.Key))
                    continue;

                double dblOffset = 0;
                if (kvp.Key.dDateCreated == null)
                    continue;

                TimeSpan tsToday = (TimeSpan)(DateTime.Now - kvp.Key.dDateCreated);
                // x is the number of hours since the creation date divided by 24. This division slows down the decay significantly for the first day.
                double dblXValue = ((double)tsToday.Hours / (double)24); 

                dblOffset = Math.Pow(Math.E, dblXValue - dblConstantShifter);
                dblOffset = 1 - dblOffset;
                dblOffset = dblOffset * dblConstantMultiplier;

                dictSparkValues.Add(kvp.Key, kvp.Value + dblOffset);
            }

            return dictSparkValues;
        }

        /// <summary>
        /// Gets a collection of all sparks that have sparkinterestvotes records in the database. Note - all sparks should have a vote record by default.
        /// Assigns the spark collected as a key in the return dictionary with values that give an absolute representation of their sort order.
        /// Higher values indicate the spark is sorted above ones with lower values. The values are influenced by user votes for the spark (up for pos, down for neg).
        /// The database constant "SortingPopularityConstant" influences the weight of the vote sorting. Increasing the value of this will weight votes more heavily.
        /// </summary>
        /// <param name="dbEntity">Database instance for which we derive constants for this algorithm.</param>
        /// <returns>Returns a dictionary with spark keys and values that indicate the sort order based on spark votes.</returns>
        private static Dictionary<sparks, double> ApplyPopularitySorting(sparkdbEntities1 dbEntity)
        {
            Dictionary<sparks, double> dictSorted = new Dictionary<sparks, double>();

            // Collection of key value pairs, key = sparkId and value = interest amount.
            Dictionary<int, double> dictInterestSparks = new Dictionary<int, double>();
            
            // Finds all of the interest votes for each spark.
            var qryForInterest = from sparkinterestvotes votes in dbEntity.sparkinterestvotes
                                 orderby votes.FKSparks
                                 select votes;

            // Interates through all of the interest votes and creates a collection of sparks that holds a value of the number of votes it has.
            foreach (sparkinterestvotes siv in qryForInterest)
            {
                // If it is a deleted vote - we still want to account for the spark being shown
                if (siv.bIsDeleted)
                {
                    if (!dictInterestSparks.ContainsKey(siv.FKSparks)) // Does not need else statement, deleted votes do not increment the count.
                        dictInterestSparks.Add(siv.FKSparks, 0);
                }
                // If it is a positive vote
                else if (siv.bIsUpVote)
                {
                    if (!dictInterestSparks.ContainsKey(siv.FKSparks))
                        dictInterestSparks.Add(siv.FKSparks, 1);
                    else
                    {
                        dictInterestSparks[siv.FKSparks] += 1; // Add a positive to the vote count.
                    }
                }
                // If it is a negative vote.
                else
                {
                    if (!dictInterestSparks.ContainsKey(siv.FKSparks))
                        dictInterestSparks.Add(siv.FKSparks, -1);
                    else
                    {
                        dictInterestSparks[siv.FKSparks] -= 1; // Add a negative to the vote count.
                    }
                }
            }


            var keys = (from r in dictInterestSparks.Keys
                        select r).ToList();

            var qryForSparksWithInterests = from sparks spark in dbEntity.sparks
                                            where keys.Contains(spark.PK)
                                            orderby spark.PK
                                            select spark;

            // Attempts to get the constant value that is multiplied to each popularity rating.
            double nConstant = 1;

            var query = (from constants constant in dbEntity.constants
                             where constant.strKey == "SortingPopularityConstant"
                             select constant).FirstOrDefault();

            if(query == null || query.strKey.Count() <= 0)
            {
                DatabaseInterfaces.BaseDatabaseInterface.LogError("Calculations Class", "There is no key in the database with the key 'SortingPopularityConstant'");
                return null;
            }

            double.TryParse(query.strValue.FirstOrDefault().ToString(), out nConstant);

            // Applies the multiplier constant to each item in the collection to modify its final value of popularity.
            foreach (sparks spark in qryForSparksWithInterests)
            {
                if (dictSorted.ContainsKey(spark))
                    continue;

                dictSorted.Add(spark, dictInterestSparks[spark.PK] * nConstant);
            }

            return dictSorted;
        }

        #endregion

        public static List<arguments> SortArgumentsByPopularity(sparkdbEntities1 db, int nSparkId)
        {
            List<arguments> lstSorted = new List<arguments>();

            Dictionary<arguments, int> dictArgVotes = PairArgumentsWithVotes(db, nSparkId);

            var qrySorted = from r in dictArgVotes
                            orderby r.Value descending
                            select r;

            if(qrySorted == null)
                return lstSorted;

            foreach (KeyValuePair<arguments, int> kvp in qrySorted)
            {
                lstSorted.Add(kvp.Key);
            }

            return lstSorted;
        }

        private static Dictionary<arguments, int> PairArgumentsWithVotes(sparkdbEntities1 db, int nSparkId)
        {
            Dictionary<arguments, int> dictArgumentVote = new Dictionary<arguments, int>();
            Dictionary<int, arguments> dictArguments = new Dictionary<int,arguments>();
            List<argumentvotes> lstVotes = new List<argumentvotes>();

            var qryAllArguments = from r in db.arguments
                                  where r.FKSparks == nSparkId
                                  select r;
            if (qryAllArguments == null)
                return dictArgumentVote;
            
            foreach (arguments argument in qryAllArguments)
            {
                if(!dictArguments.ContainsKey(argument.PK))
                    dictArguments.Add(argument.PK, argument);
                if (!dictArgumentVote.ContainsKey(argument))
                    dictArgumentVote.Add(argument, 0);
            }

            var qryVotes = from r in db.argumentvotes
                           where dictArguments.Keys.Contains(r.FKArguments)
                           select r;
            if (qryVotes == null)
                return dictArgumentVote;

            foreach (argumentvotes vote in qryVotes)
            {
                if (!dictArguments.ContainsKey(vote.FKArguments))
                    continue;

                if (vote.bIsUpvote)
                    dictArgumentVote[dictArguments[vote.FKArguments]]++;
                else
                    dictArgumentVote[dictArguments[vote.FKArguments]]--;
            }

            return dictArgumentVote;
        }

    }
}