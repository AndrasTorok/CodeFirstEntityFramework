using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public class Ado : IDisposable
    {        
        public DbProviderFactory factory { get; set; }
        public DbConnection connection { get; set; }
        public DbCommand command { get; set; }

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);     
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    connection.Dispose();
                    command.Dispose();
                }

                disposed = true;
            }
        }

        #endregion
    }
}
