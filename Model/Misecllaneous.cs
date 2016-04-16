using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Model
{
    public class Misecllaneous : ICloneable
    {
        public double Rate { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
