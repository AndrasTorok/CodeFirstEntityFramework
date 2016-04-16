using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class EntityChanges<T>
    {
        public List<T> Added { get; set; }
        public List<T> Updated { get; set; }
        public List<T> Deleted { get; set; }
    }
}
