using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Models;
using System.Data;

namespace Spark.Classes
{
    public static class Analytics
    {
        /// <summary>
        /// This method finds the breakdown of how each gender feels about different spark categories.
        /// This takes the total number of up votes minus downvotes that a particular gender gives to a category and divides by the total up votes and down votes respectively across all categories.
        /// This data will have to be somehow collected and shown in a meaningful way.
        /// Current this method returns a datatable with columns: Gender, SubjectMatter, Count
        /// This will have records, 2 for each subject matter, that show the gender-based weighted average of the spark interest votes.
        /// The average is (#upvotes - #downvotes) / #totalvotes and is repeated for each gender and subject matter combination.
        /// </summary>
        /// <returns></returns>
        public static DataTable CompareGenderCategories(sparkdbEntities dbEntity)
        {
            DataTable dtReturn = new DataTable();
            DataColumn dcGender = new DataColumn("Gender");
            DataColumn dcSubjectMatter = new DataColumn("SubjectMatter");
            DataColumn dcCount = new DataColumn("Count");
            dtReturn.Columns.AddRange(new DataColumn[]{dcGender, dcSubjectMatter, dcCount});

            // Iterates through each gender.
            foreach (int nGender in (from r in dbEntity.genders select r.PK))
            {
                DataRow dr = dtReturn.NewRow();
                dr[dcGender] = nGender;

                int nTotalVotes = (from r in dbEntity.sparkinterestvotes
                                     join p in dbEntity.profiles on r.FKProfiles equals p.PK
                                     where p.FKGenders == nGender
                                     select r).Count();

                // Iterates through each category.
                foreach (int nSubjectMatter in (from r in dbEntity.subjectmatters select r.PK))
                {
                    dr[dcSubjectMatter] = nSubjectMatter;
                    // Performs a query to gather all up or down votes for this selected gender and subject matter combination.
                    var qryMain = from siv in dbEntity.sparkinterestvotes
                                  join p in dbEntity.profiles on siv.FKProfiles equals p.PK
                                  join s in dbEntity.sparks on siv.FKSparks equals s.PK
                                  where p.FKGenders == nGender && s.FKSubjectMatters == nSubjectMatter
                                  select siv;

                    // Collects the positive/negative scale of number of votes for this particular gender/subjectmatter.
                    int nCountVotes = 0;
                    foreach (sparkinterestvotes siv in qryMain)
                    {
                        if (siv.bIsUpVote)
                            nCountVotes++;
                        else
                            nCountVotes--;
                    }

                    // Table value = (upvotes - downvotes) / total votes, this gets the weighted average. 
                    dr[dcCount] = (double)(nCountVotes / nTotalVotes);
                }
            }

            return dtReturn;
        }

        private static void TestMethod(sparkdbEntities dbEntity)
        {
            
        }
    }
}