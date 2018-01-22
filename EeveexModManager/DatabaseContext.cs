using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using EeveexModManager.Classes;
using System.IO;

namespace EeveexModManager
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ModDatabase> NexusModList { get; set; }
        public DbSet<ModDatabase> ArchiveModList { get; set; }


        public DatabaseContext()
        {
            Database.Migrate();
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($@"Data Source={Directory.GetCurrentDirectory()}\Database.db");

        }
    }
}
