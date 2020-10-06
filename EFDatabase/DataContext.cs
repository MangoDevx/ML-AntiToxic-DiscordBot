using Microsoft.EntityFrameworkCore;
using MLNetDBot.EFDatabase.EFModels;
using System;

namespace MLNetDBot.EFDatabase
{
    public class DataContext : DbContext
    {
        public DbSet<MlMessage> MlMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                .UseSqlite(@$"DataSource={Environment.CurrentDirectory}/ToxicMsgsDB.db");
        }
    }
}
