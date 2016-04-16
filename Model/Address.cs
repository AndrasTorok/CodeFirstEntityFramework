using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Model
{
    public class Address : ICloneable
    {
        public string Country { get; set; }
        public string Locality { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string Coordinate { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
