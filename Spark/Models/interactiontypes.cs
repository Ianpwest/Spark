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
    
    public partial class interactiontypes
    {
        public interactiontypes()
        {
            this.interactionlog = new HashSet<interactionlog>();
        }
    
        public int PK { get; set; }
        public string strName { get; set; }
    
        public virtual ICollection<interactionlog> interactionlog { get; set; }
    }
}
