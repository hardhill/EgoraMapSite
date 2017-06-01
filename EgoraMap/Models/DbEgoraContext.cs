using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EgoraMap.Models
{
    public class DbEgoraContext:DbContext
    {
        
        public DbSet<Route> Routes { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string con = Startup.Configuration.GetSection("ConnectionStrings:DbEgoraContext").Value;
            optionsBuilder.UseSqlServer(con);
        }
    }
}
