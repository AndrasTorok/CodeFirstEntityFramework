using CodeFirstEntityFramework.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class PersonRepositoryAsync : PkLessRepositoryAsync<Person, Person>
    {
        public PersonRepositoryAsync(ConnectionStringSettings connStringSettings)
            : base(connStringSettings, new string[] { "PersonId" }, new string[] { "PersonId" })
        {

        }

        protected override Person ModelFromEntity(Person entity)
        {
            return entity.Clone() as Person;
        }

        protected override Person EntityFromModel(Person model)
        {
            return model.Clone() as Person;
        }

        protected override string TableName
        {
            get { return "People"; }
        }
    }
}
