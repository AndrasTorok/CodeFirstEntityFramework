using CodeFirstEntityFramework.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Context
{
    public class EFContext : DbContext
    {
        public EFContext() : base("name=CodeFirstEntityFramework") { }

        public DbSet<Person> People { get; set; }
        //public DbSet<Company> Companies { get; set; }
    }
}
