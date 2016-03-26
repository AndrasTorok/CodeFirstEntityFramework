using CodeFirstEntityFramework.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class PersonRepository : PkLessRepository<Person, Person>
    {
        public PersonRepository(ConnectionStringSettings connStringSettings)
            : base(connStringSettings, new string[] { "PersonId" }, new string[] { "PersonId" })
        {

        }

        public override Person ModelFromEntity(Person entity)
        {
            return entity.Clone() as Person;
        }

        public override Person EntityFromModel(Person model)
        {
            return model.Clone() as Person;
        }

        public override string TableName
        {
            get { return "People"; }
        }
    }
}
