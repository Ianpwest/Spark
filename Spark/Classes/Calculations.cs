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
        /// <summary>
        /// Used to calculate a particular user's total influence.
        /// </summary>
        /// <returns></returns>
        public static double Influence(UserProfile userCurrent, sparkdbEntities dbModel, int nSubjectMatterId)
        {
            double dblInfluence = 0;

            dblInfluence += GetBaseInfluence(userCurrent, dbModel, nSubjectMatterId);
            dblInfluence += GetUserAddons(userCurrent, dbModel, nSubjectMatterId);
            dblInfluence += GetSpread(userCurrent, dbModel, nSubjectMatterId);

            return dblInfluence;
        }

        private static double GetBaseInfluence(UserProfile userCurrent, sparkdbEntities dbModel, int nSubjectMatterId)
        {
            double dblBase = 0;

            // Returns the number of positive votes minus the number of negative votes from the gains table to get the absolute scale.
            dblBase  = (from influencegains influencegain in dbModel.influencegains
                        where influencegain.FKProfilesReceived == userCurrent.UserId && influencegain.bIsPositive == true && influencegain.FKSubjectMatters == nSubjectMatterId
                        select influencegain).Count() -
                        (from influencegains influencegain in dbModel.influencegains
                        where influencegain.FKProfilesReceived == userCurrent.UserId && influencegain.bIsPositive == false && influencegain.FKSubjectMatters == nSubjectMatterId
                        select influencegain).Count();

            int nConstant = 1;
            if(int.TryParse((from constants constant in dbModel.constants
                            where constant.strKey == "InfluenceBaseConstant"
                            select constant.strValue).FirstOrDefault(), out nConstant))
                dblBase = dblBase * nConstant;

            return dblBase;
        }

        private static double GetUserAddons(UserProfile userCurrent, sparkdbEntities dbModel, int nSubjectMatterId)
        {
            double dblUserAddon = 0;

            // Queries for all of the userIds of people who contributed to the initial user's influence in the gains table.
            IEnumerable<int> qryAllContributors = from influencegains influence in dbModel.influencegains
                                                  where influence.FKProfilesReceived == userCurrent.UserId
                                                  select influence.FKProfilesContributor;
            
            // Attempts to determine the constant to use for userAddon values.
            int nConstant = 1;
            int.TryParse((from constants constant in dbModel.constants
                          where constant.strKey == "InfluenceUserAddonConstant"
                          select constant.strValue).FirstOrDefault(), out nConstant);
            foreach (int n in qryAllContributors)
            {
                // Finds the base influence value for each of the users who contributed to the current user's influence.
                // Multiplies the base value found by a constant, then adds it to the running total.
                dblUserAddon += (GetBaseInfluence(new UserProfile() { UserId = n, UserName = string.Empty }, dbModel, nSubjectMatterId) * nConstant) ;
            }
            
            return dblUserAddon;
        }

        private static double GetSpread(UserProfile userCurrent, sparkdbEntities dbModel, int nSubjectMatterId)
        {
            double dblSpread = 0;

            IEnumerable<int> qrySpreadValues = from subjectmatterspreads spread in dbModel.subjectmatterspreads
                                               where spread.FKSubjectMattersSpread == nSubjectMatterId
                                               select spread.nValue;
            foreach (int n in qrySpreadValues)
            {
                dblSpread += n;
            }

            return dblSpread;
        }
    }
}