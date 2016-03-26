using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Abstract
{
    public interface IUnitOfWork
    {
        IPersonRepository Persons { get; }

        Task Commit();
    }
}
