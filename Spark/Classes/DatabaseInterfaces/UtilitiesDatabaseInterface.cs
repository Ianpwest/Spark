using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spark.Classes
{

    public class UtilitiesDatabaseInterface
    {
        /// <summary>
        /// Local Instance of the database model
        /// </summary>
        private static Spark.Models.sparkdbEntities m_db = new Models.sparkdbEntities();

        /// <summary>
        /// TODO: Add commenting here
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetSubjectMatters()
        {
            Dictionary<int, string> dictSubjectMatters = new Dictionary<int, string>();

            var qrySubjectMatters = from r in m_db.subjectmatters
                                    select new KeyValuePair<int, string>(r.PK, r.strName);

            foreach (KeyValuePair<int, string> kvp in qrySubjectMatters)
            {
                if (dictSubjectMatters.ContainsKey(kvp.Key))
                    continue;

                dictSubjectMatters.Add(kvp.Key, kvp.Value);
            }

            return dictSubjectMatters;
        }
    }
}