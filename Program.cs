using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFirstEntityFramework.Context;
using CodeFirstEntityFramework.Model;
using CodeFirstEntityFramework.Abstract;
using CodeFirstEntityFramework.Concrete;

namespace CodeFirstEntityFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            UseUnitOfWork().Wait();

        }

        static void UseEfDirectly()
        {
            using (EFContext context = new EFContext())
            {
                context.Database.CreateIfNotExists();

                context.People.Add(new Person { FirstName = "Peter", LastName = "Gabriel" });
                context.SaveChanges();
            };
        }

        static async Task UseUnitOfWork()
        {
            IUnitOfWork uow = new UnitOfWork(new EFContext());

            Person firstPerson = uow.Persons.Query().FirstOrDefault();

            firstPerson.FirstName += "1";

            await uow.Commit();
        }
    }
}
