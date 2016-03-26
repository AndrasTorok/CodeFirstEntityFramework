using CodeFirstEntityFramework.Abstract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Concrete
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DbContext context;
        private bool disposed = false;

        public UnitOfWork(DbContext context)
        {
            this.context = context;
            Persons = new PersonRepository(context);
        }

        public IPersonRepository Persons { get; private set; }

        public async Task Commit()
        {
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing) context.Dispose();
            this.disposed = true;
        }        
    }
}
