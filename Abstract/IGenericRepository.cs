using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Abstract
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> Query();
        void Add(T entity);
        void Remove(T entity);

    }
}
