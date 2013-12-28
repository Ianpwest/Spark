using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Classes.DatabaseInterfaces;

namespace Spark.Classes
{

    public class UtilitiesDatabaseInterface : BaseDatabaseInterface
    {   
        /// <summary>
        /// Provides a list of all available broad categories for the spark categorization.
        /// </summary>
        /// <returns>Dictionary containing a indexed list of all broad categories whose keys consist of the primary key from the database for each broad category and
        /// values equal to the respective names of the broad categories.</returns>
        public static Dictionary<int, string> GetSubjectMatters()
        {
            Dictionary<int, string> dictSubjectMatters = new Dictionary<int, string>();
            
            var qrySubjectMatters = from r in m_db.subjectmatters
                                    select r;

            
            foreach (Models.subjectmatters sm in qrySubjectMatters)
            {
                if (dictSubjectMatters.ContainsKey(sm.PK))
                    continue;

                dictSubjectMatters.Add(sm.PK, sm.strName);
            }

            return dictSubjectMatters;
        }

        public static string GetSubjectMatterImageName(int nSubjectMatterId)
        {
            return (from r in m_db.subjectmatters
                    where r.PK == nSubjectMatterId
                    select r.strImageName).FirstOrDefault();
        }

        public static Dictionary<int, string> GetTagIdAndNames()
        {
            var qryAllTags = from r in m_db.categories
                             select r;
            if (qryAllTags == null || qryAllTags.Count() < 1)
            {
                LogNonUserError("Unable to get tags from the database.", "", "", "DbUtil class", "GetTagIdAndNames", "qryAllTags");
                return null;
            }

            Dictionary<int, string> dictIdNames = new Dictionary<int, string>();

            foreach (Spark.Models.categories cat in qryAllTags)
            {
                if (dictIdNames.ContainsKey(cat.PK))
                    continue;

                dictIdNames.Add(cat.PK, cat.strName);
            }

            return dictIdNames;
        }

        public static Dictionary<int, string> GetTagIdAndImageNames()
        {
            var qryAllTags = from r in m_db.categories
                             select r;
            if (qryAllTags == null || qryAllTags.Count() < 1)
            {
                LogNonUserError("Unable to get tags from the database.", "", "", "DbUtil class", "GetTagIdAndImageNames", "qryAllTags");
                return null;
            }

            Dictionary<int, string> dictIdImg = new Dictionary<int, string>();

            foreach (Spark.Models.categories cat in qryAllTags)
            {
                if (dictIdImg.ContainsKey(cat.PK))
                    continue;

                dictIdImg.Add(cat.PK, cat.strImageName);
            }

            return dictIdImg;
        }

        public static bool GenerateTagInfoForSpark(Spark.Models.SparkCreateModel sparkModel)
        {
            var qryAllTags = from r in m_db.categories
                             where r.PK >= 0
                             select r;
            if(qryAllTags == null || qryAllTags.Count() < 1)
            {
                LogNonUserError("Unable to get tags from the database.", "", "", "DbUtil class", "GenerateTagInfoForSpark", "qryAllTags");
                return false;
            }
            Dictionary<int, string> dictIdNames = new Dictionary<int, string>();
            Dictionary<int, string> dictIdImg = new Dictionary<int, string>();

            foreach(Spark.Models.categories cat in qryAllTags)
            {
                if(dictIdImg.ContainsKey(cat.PK) || dictIdNames.ContainsKey(cat.PK))
                    continue;

                dictIdNames.Add(cat.PK, cat.strName);
                dictIdImg.Add(cat.PK, cat.strImageName);
            }

            sparkModel.TagIdAndNames = dictIdNames;
            sparkModel.TagIdAndImages = dictIdImg;

            return false;
        }
    }
}