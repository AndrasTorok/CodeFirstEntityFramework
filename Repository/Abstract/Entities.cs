using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class Entities<T>
        where T : class, ICloneable
    {
        private List<string> pKeys;

        public Entities(IEnumerable<string> pKeys)
        {
            this.pKeys = pKeys.ToList();
        }

        public EntityChanges<T> EntityChanges(IEnumerable<T> entities, IEnumerable<T> comparedEntities)
        {
            List<T> addedEntities = new List<T>(),
                updatedEntities = new List<T>(),
                deletedEntities = new List<T>();

            entities.ToList().ForEach(entity =>
            {
                T comparedEntity = findByPK(comparedEntities, entity);

                if (comparedEntity == null) addedEntities.Add(entity);
                else if (!this.equals(entity, comparedEntity)) updatedEntities.Add(entity);
            });

            comparedEntities.ToList().ForEach(comparedEntity =>
            {
                if (findByPK(entities, comparedEntity) == null) deletedEntities.Add(comparedEntity);
            });

            return new EntityChanges<T>
            {
                Added = addedEntities,
                Updated = updatedEntities,
                Deleted = deletedEntities
            };
        }

        private T findByPK(IEnumerable<T> entities, T entityToFind)
        {
            Type type = typeof(T);
            T entity = entities.FirstOrDefault((e) => this.pKeys.All(pk => type.GetProperty(pk).GetValue(e).Equals(type.GetProperty(pk).GetValue(entityToFind))));

            return entity;
        }

        private bool equals(T first, T second)
        {
            Type type = typeof(T);
            List<string> properties = type.GetProperties().Where(p => !p.GetMethod.IsVirtual).Select(m => m.Name).ToList();

            return properties.All(p => type.GetProperty(p).GetValue(first).Equals(type.GetProperty(p).GetValue(second)));            
        }
    }    
}
