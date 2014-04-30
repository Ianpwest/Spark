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
        [Display(Name = "Category")]
        public int SubjectMatterId {get;set;}

        [Required, StringLength(23, ErrorMessage="The title is limited to 23 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Topic")]
        public string Topic { get; set; }

        public string UserId { get; set; }

        public SelectList SubjectMattersAll { get; set; }
        public Dictionary<int, string> TagIdAndImages { get; set; }
        public Dictionary<int, string> TagIdAndNames { get; set; }

        public int Tag1 { get; set; }
        public int Tag2 { get; set; }
        public int Tag3 { get; set; }
        public int Tag4 { get; set; }
        public int Tag5 { get; set; }

        public ArgumentEntryType ArgEntryType { get; set; }
    }
    public enum ArgumentEntryType
    {
        Neither = 0,
        Agree = 1,
        Disagree = 2
    }

    public class SparkTileModel
    {
        public int PK { get; set; }
        public string Topic { get; set; }
        public bool UserVoted { get; set; }
        public bool VoteIsUpvote { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
    }
}