using CodeFirstEntityFramework.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class AddressRepository : PkLessRepository<Address>
    {
        public AddressRepository(ConnectionStringSettings connStringSettings)
            : base(connStringSettings, new string[] { "Country", "Locality", "Street", "Number" })
        {

        }

        protected override string TableName
        {
            get { return "Address"; }
        }
    }
}
