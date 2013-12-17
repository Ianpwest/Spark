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
        public static double Influence(accounts userCurrent, sparkdbEntities1 dbEntity, int nSubjectMatterId)
        {
            double dblInfluence = 0;

            dblInfluence += GetBaseInfluence(userCurrent, dbEntity, nSubjectMatterId);
            dblInfluence += GetUserAddons(userCurrent, dbEntity, nSubjectMatterId);
            dblInfluence += GetSpread(userCurrent, dbEntity, nSubjectMatterId);

            return dblInfluence;
        }

        /// <summary>
        /// Calculates base influence for a specific user in a specific broad category. This influence is base and is not affected by any other influencial gains.
        /// </summary>
        /// <param name="userCurrent">Account Id for user.</param>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nSubjectMatterId">Broad category against which to calculate.</param>
        /// <returns>Double value that represents the base influence number.</returns>
        private static double GetBaseInfluence(accounts userCurrent, sparkdbEntities1 dbEntity, int nSubjectMatterId)
        {
            double dblBase = 0;

            // Returns the number of positive votes minus the number of negative votes from the gains table to get the absolute scale.
            dblBase  = (from influencegains influencegain in dbEntity.influencegains
                        where influencegain.FKAccountsReceived == userCurrent.PK && influencegain.bIsPositive == true && influencegain.FKSubjectMatters == nSubjectMatterId
                        select influencegain).Count() -
                        (from influencegains influencegain in dbEntity.influencegains
                        where influencegain.FKAccountsReceived == userCurrent.PK && influencegain.bIsPositive == false && influencegain.FKSubjectMatters == nSubjectMatterId
                        select influencegain).Count();

            double nConstant = 1;
            if(double.TryParse((from constants constant in dbEntity.constants
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
        private static double GetUserAddons(accounts userCurrent, sparkdbEntities1 dbEntity, int nSubjectMatterId)
        {
            double dblUserAddon = 0;

            // Queries for all of the userIds of people who contributed to the initial user's influence in the gains table.
            IEnumerable<int> qryAllContributors = from influencegains influence in dbEntity.influencegains
                                                  where influence.FKAccountsReceived == userCurrent.PK
                                                  select influence.FKAccountsReceived;
            
            // Attempts to determine the constant to use for userAddon values.
            double nConstant = 1;
            double.TryParse((from constants constant in dbEntity.constants
                            where constant.strKey == "InfluenceUserAddonConstant"
                            select constant.strValue).FirstOrDefault(), out nConstant);
            foreach (int n in qryAllContributors)
            {
                // Finds the base influence value for each of the users who contributed to the current user's influence.
                // Multiplies the base value found by a constant, then adds it to the running total.
                dblUserAddon += (GetBaseInfluence(new accounts() { PK = n, strUserName = string.Empty }, dbEntity, nSubjectMatterId) * nConstant) ;
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
        private static double GetSpread(accounts userCurrent, sparkdbEntities1 dbEntity, int nSubjectMatterId)
        {
            double dblSpread = 0;

            IEnumerable<int> qrySpreadValues = from subjectmatterspreads spread in dbEntity.subjectmatterspreads
                                               where spread.FKSubjectMattersSpread == nSubjectMatterId
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
        /// Provides a list of sparks sorted by broad category given the broad category Id.
        /// </summary>
        /// <param name="dbEntity">Database entity.</param>
        /// <param name="nSubjectMatterId"></param>
        /// <returns></returns>
        public static List<sparks> SortSparksBySubject(sparkdbEntities1 dbEntity, int nSubjectMatterId)
        {
            List<sparks> lstSorted = new List<sparks>();

            Dictionary<sparks, double> dictPopularitySorted = ApplyPopularitySorting(dbEntity);

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

        public static List<sparks> SortSparksByUserInterest(sparkdbEntities1 dbEntity)
        {
            List<sparks> lstSorted = new List<sparks>();

            return lstSorted;
        }

        private static Dictionary<sparks, double> ApplyDecayAlgorithm(Dictionary<sparks, double> dictSparks, sparkdbEntities1 dbEntity)
        {
            Dictionary<sparks, double> dictSparkValues = new Dictionary<sparks, double>();

            // Creating decay function that will give negative values to the spark depending on how long it has been since created.
            // P = C(1 - e^(x-k))  where C is a multiplier constant and k is a shifter constant.

            double dblConstantMultiplier = 1;
            double dblConstantShifter = 0;

            double.TryParse((from constants constant in dbEntity.constants
                             where constant.strKey == "SortingDecayMultiplier"
                             select constant.strValue).FirstOrDefault(), out dblConstantMultiplier);
            double.TryParse((from constants constant in dbEntity.constants
                             where constant.strKey == "SortingDecayShifter"
                             select constant.strValue).FirstOrDefault(), out dblConstantShifter);

            foreach (KeyValuePair<sparks, double> kvp in dictSparks)
            {
                if (dictSparkValues.ContainsKey(kvp.Key))
                    continue;

                double dblOffset = 0;
                if (kvp.Key.dDateCreated == null)
                    continue;

                TimeSpan tsToday = (TimeSpan)(DateTime.Now - kvp.Key.dDateCreated);
                int nXValue = tsToday.Days;

                dblOffset = Math.Pow(Math.E, nXValue - dblConstantShifter);
                dblOffset = 1 - dblOffset;
                dblOffset = dblOffset * dblConstantMultiplier;

                dictSparkValues.Add(kvp.Key, kvp.Value + dblOffset);
            }

            return dictSparkValues;
        }

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
                // If it is a positive vote
                if (siv.bIsUpVote)
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

            // Selects all sparks in the database that are contained in the popularity sorted dictionary and sorts them by their Id.
            var qryForSparksWithInterests = from sparks spark in dbEntity.sparks
                                            where dictInterestSparks.ContainsKey(spark.PK)
                                            orderby spark.PK
                                            select spark;

            // Attempts to get the constant value that is multiplied to each popularity rating.
            double nConstant = 1;
            double.TryParse((from constants constant in dbEntity.constants
                             where constant.strKey == "SortingPopularityConstant"
                             select constant).FirstOrDefault().strValue, out nConstant);

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

    }
}