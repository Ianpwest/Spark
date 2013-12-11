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
        public int SubjectMatterId;

        [Required]
        [Display(Name = "Topic")]
        public string Topic;

        [Required]
        [Display(Name = "Description")]
        public string Description;

        public string UserId;

        public SelectList SubjectMattersAll;
    }
}