using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EgoraMap.Models
{
    public class DbEgoraContext:DbContext
    {
        //public DbEgoraContext(DbContextOptions<DbEgoraContext> options) : base(options)
        //{

        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=SAPPHIRE;Initial Catalog=EgoraDB;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public DbSet<Route> Routes { get; set; }
        public DbSet<Photo> Photos { get; set; }
    }
}
