using CodeFirstEntityFramework.Context;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirstEntityFramework.Repository
{
    public abstract class PkLessRepository<TEntity, TModel> : PkLessRepositoryBase<TEntity, TModel>
        where TEntity : class, new()
        where TModel : class, new()
    {
        public PkLessRepository(ConnectionStringSettings connStringSettings, string[] keyColumns, string[] identityColumns = null) :
            base(connStringSettings, keyColumns, identityColumns)
        {

        }
        
        public virtual TModel GetById(params object[] primaryKeys)
        {
            return ExecuteQuery<TModel>(() =>
            {
                return String.Format("select {0} from {1} where {2};", ColumnNames(columns),
                    TableName, FilteringParameters(primaryKeys));
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

        public virtual bool Update(TModel model)
        {
            return ExecuteNonQuery(() =>
            {
                string commandText = String.Format("update {0} set {1} where {2};", TableName,
                    UpdateParameters(model), FilteringParameters(model));

                return commandText;
            });
        }

        public virtual bool Save(TModel model)
        {
            return ExecuteNonQuery(() =>
            {
                string commandText = String.Format("insert into {0} ({1}) values({2});", TableName,
                        ColumnNames(insertColumns), InsertParameters(model));

                return commandText;
            });
        }

        public virtual bool Delete(object[] primaryKeyValues)
        {
            return ExecuteNonQuery(() =>
            {
                string commandText = String.Format("delete {0} where {1};", TableName,
                        FilteringParameters(primaryKeyValues));

                return commandText;
            });            
        }
    }
}