﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class sparkdbEntities : DbContext
    {
        public sparkdbEntities()
            : base("name=sparkdbEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<accounts> accounts { get; set; }
        public DbSet<arguments> arguments { get; set; }
        public DbSet<categories> categories { get; set; }
        public DbSet<cities> cities { get; set; }
        public DbSet<comments> comments { get; set; }
        public DbSet<commentvotes> commentvotes { get; set; }
        public DbSet<constants> constants { get; set; }
        public DbSet<countries> countries { get; set; }
        public DbSet<educationlevels> educationlevels { get; set; }
        public DbSet<errorlog> errorlog { get; set; }
        public DbSet<genders> genders { get; set; }
        public DbSet<heat> heat { get; set; }
        public DbSet<influencegains> influencegains { get; set; }
        public DbSet<interactionlog> interactionlog { get; set; }
        public DbSet<interactiontypes> interactiontypes { get; set; }
        public DbSet<politicalideologies> politicalideologies { get; set; }
        public DbSet<profiles> profiles { get; set; }
        public DbSet<religions> religions { get; set; }
        public DbSet<sparkinterestvotes> sparkinterestvotes { get; set; }
        public DbSet<sparks> sparks { get; set; }
        public DbSet<sparkscategories> sparkscategories { get; set; }
        public DbSet<statesprovidences> statesprovidences { get; set; }
        public DbSet<subjectmatters> subjectmatters { get; set; }
        public DbSet<subjectmatterspreads> subjectmatterspreads { get; set; }
        public DbSet<transactions> transactions { get; set; }
        public DbSet<transactionstates> transactionstates { get; set; }
        public DbSet<transactiontypes> transactiontypes { get; set; }
    }
}
