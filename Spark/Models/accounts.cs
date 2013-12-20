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
    
    public partial class accounts
    {
        public accounts()
        {
            this.commentvotes = new HashSet<commentvotes>();
            this.arguments = new HashSet<arguments>();
            this.errorlog = new HashSet<errorlog>();
            this.heat = new HashSet<heat>();
            this.influencegains = new HashSet<influencegains>();
            this.influencegains1 = new HashSet<influencegains>();
            this.interactionlog = new HashSet<interactionlog>();
            this.sparkinterestvotes = new HashSet<sparkinterestvotes>();
            this.sparks = new HashSet<sparks>();
            this.transactions = new HashSet<transactions>();
            this.profiles = new HashSet<profiles>();
            this.comments = new HashSet<comments>();
        }
    
        public int PK { get; set; }
        public string strUserName { get; set; }
        public string strPassword { get; set; }
        public string strSalt { get; set; }
        public bool bIsActivated { get; set; }
        public string strEmail { get; set; }
        public Nullable<System.Guid> gActivationGUID { get; set; }
    
        public virtual ICollection<commentvotes> commentvotes { get; set; }
        public virtual ICollection<arguments> arguments { get; set; }
        public virtual ICollection<errorlog> errorlog { get; set; }
        public virtual ICollection<heat> heat { get; set; }
        public virtual ICollection<influencegains> influencegains { get; set; }
        public virtual ICollection<influencegains> influencegains1 { get; set; }
        public virtual ICollection<interactionlog> interactionlog { get; set; }
        public virtual ICollection<sparkinterestvotes> sparkinterestvotes { get; set; }
        public virtual ICollection<sparks> sparks { get; set; }
        public virtual ICollection<transactions> transactions { get; set; }
        public virtual ICollection<profiles> profiles { get; set; }
        public virtual ICollection<comments> comments { get; set; }
    }
}