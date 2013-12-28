using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Spark.Models
{
    public class SparkCreateModel
    {
        [Required]
        [Display(Name = "Subject Matter")]
        public int SubjectMatterId {get;set;}

        [Required]
        [Display(Name = "Topic")]
        public string Topic { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public string UserId { get; set; }

        public SelectList SubjectMattersAll { get; set; }
        public Dictionary<int, string> TagIdAndImages { get; set; }
        public Dictionary<int, string> TagIdAndNames { get; set; }

        public int Tag1 { get; set; }
        public int Tag2 { get; set; }
        public int Tag3 { get; set; }
        public int Tag4 { get; set; }
        public int Tag5 { get; set; }
    }
}