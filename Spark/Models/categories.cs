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
    
    public partial class categories
    {
        public categories()
        {
            this.sparkscategories = new HashSet<sparkscategories>();
            this.sparkscategories1 = new HashSet<sparkscategories>();
            this.sparkscategories2 = new HashSet<sparkscategories>();
        }
    
        public int PK { get; set; }
        public string strName { get; set; }
    
        public virtual ICollection<sparkscategories> sparkscategories { get; set; }
        public virtual ICollection<sparkscategories> sparkscategories1 { get; set; }
        public virtual ICollection<sparkscategories> sparkscategories2 { get; set; }
    }
}
