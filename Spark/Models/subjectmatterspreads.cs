//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Spark.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class subjectmatterspreads
    {
        public int PK { get; set; }
        public int FKSubjectMattersContributor { get; set; }
        public int FKSubjectMattersSpread { get; set; }
        public int nValue { get; set; }
    
        public virtual subjectmatters subjectmatters { get; set; }
        public virtual subjectmatters subjectmatters1 { get; set; }
    }
}