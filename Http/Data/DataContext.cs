using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Http.Data
{
    public class DataContext : DbContext
    {
        public DbSet<NpUser> NpUsers { get; set; }

        public DataContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var emailConfig = JsonSerializer.Deserialize<dynamic>(
                    System.IO.File.ReadAllText("emailconfig.json")
                );

            String connectionString = emailConfig.GetProperty("dbms")
                .GetProperty("planetScale").GetString();

            optionsBuilder.UseMySql(connectionString, 
                new MySqlServerVersion(new Version(8,0,23)));
        }
    }
}
