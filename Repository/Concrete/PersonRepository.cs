using CodeFirstEntityFramework.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class PersonRepository : PkLessRepository<Person>
    {
        public PersonRepository(ConnectionStringSettings connStringSettings)
            : base(connStringSettings, new string[] { "PersonId" }, new string[] { "PersonId" })
        {

        }

        protected override string TableName
        {
            get { return "People"; }
        }
    }
}
