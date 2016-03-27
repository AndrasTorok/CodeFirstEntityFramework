using CodeFirstEntityFramework.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Model
{
    public class PersonModel
    {
        [ColumnNameMapping("PersonId", "PersonIdInModel")]
        public int PersonIdInModel { get; set; }

        [ColumnNameMapping("FirstName", "FirstNameInModel")]
        public string FirstNameInModel { get; set; }

        [ColumnNameMapping("LastName", "LastNameInModel")]
        public string LastNameInModel { get; set; }
    }
}
