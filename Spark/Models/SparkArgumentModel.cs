using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Spark.Models
{
    public class SparkArgumentModel
    {
        public int id { get; set; }

        public string strUserName { get; set; }

        public int nCommentCount { get; set; }

        public int nUpVote { get; set; }

        public int nDownVote { get; set; }

        public int nInfluenceScore { get; set; }

        [Required]
        public string strConclusion { get; set; }

        public int nArgumentScore { get; set; }

        [Required]
        public string strCitations { get; set; }

        public bool bIsAgree { get; set; }

        [Required]
        public string strArgument { get; set; }
    }
}