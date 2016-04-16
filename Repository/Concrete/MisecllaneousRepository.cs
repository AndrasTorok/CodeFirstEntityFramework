using CodeFirstEntityFramework.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class MisecllaneousRepository : SingleLineRepository<Misecllaneous>
    {
        public MisecllaneousRepository(ConnectionStringSettings connStringSettings)
            : base(connStringSettings)
        {

        }

        protected override string TableName
        {
            get { return "Misecllaneous"; }
        }
    }
}
