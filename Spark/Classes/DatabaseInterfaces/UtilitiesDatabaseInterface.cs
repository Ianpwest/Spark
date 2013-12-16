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
    }
}