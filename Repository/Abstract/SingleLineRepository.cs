using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public abstract class SingleLineRepository<TModel> : RepositoryBase<TModel>
        where TModel : class, ICloneable, new()
    {
        public SingleLineRepository(ConnectionStringSettings connStringSettings, string[] identityColumns = null)
            : base(connStringSettings, identityColumns)
        {

        }

        public virtual TModel Get()
        {
            return ExecuteQuery<TModel>((command) =>
            {
                return String.Format("select {0} from {1};", ColumnNames(columns), TableName);
            }, (reader) =>
            {
                TModel model = default(TModel);

                if (reader.HasRows)
                {
                    reader.Read();                                      //we read ony the first row
                    model = GetModelFromReader(reader);
                }

                return model;
            });
        }

        public virtual bool Save(TModel model)
        {            
            return ExecuteNonQuery((command) =>
            {
                string commandText = String.Format("delete from {0};insert into {0} ({1}) values({2});", TableName,
                        ColumnNames(insertColumns), InsertParameters(command, model));

                return commandText;
            });
        }
    }
}
