using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class ColumnNameMappingAttribute : Attribute
    {
        private readonly string entityName;
        private readonly string modelName;

        public ColumnNameMappingAttribute(string entityName, string modelName)
        {
            this.entityName = entityName;
            this.modelName = modelName;
        }

        public string EntityName { get { return entityName; } }
        public string ModelName { get { return modelName; } }
    }
}
