using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Microsoft.EntityFrameworkCore;

namespace EeveexModManager
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ModDatabase> ModList { get; set; }

        public DatabaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Database.db");

        }
    }

    public class ModDatabase
    {
        public int ModDatabaseId { get; set; }

        public ulong Id { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Installed { get; set; }
        public string SourceArchive { get; set; }

        public int ModCategoryIndex { get; set; }
    }

}
