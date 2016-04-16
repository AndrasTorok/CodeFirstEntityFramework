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
    public abstract class PkLessRepository<TModel> : PkLessRepositoryBase<TModel>
        where TModel : class,ICloneable, new()
    {
        public PkLessRepository(ConnectionStringSettings connStringSettings, string[] keyColumns, string[] identityColumns = null) :
            base(connStringSettings, keyColumns, identityColumns)
        {

        }

        public virtual List<TModel> GetAll(params KeyValuePair<string, object>[] filterKpvs)
        {
            return ExecuteQuery<List<TModel>>((command) =>
            {
                return String.Format("select {0} from {1};", ColumnNames(columns), TableName);
            }, (reader) =>
            {
                List<TModel> result = new List<TModel>();

                while (reader.HasRows && reader.Read())
                {
                    result.Add(GetModelFromReader(reader));
                }

                return result;
            });
        }
        
        public virtual TModel GetById(params object[] primaryKeys)
        {
            return ExecuteQuery<TModel>((command) =>
            {
                return String.Format("select {0} from {1} where {2};", ColumnNames(columns),
                    TableName, FilteringParameters(command, primaryKeys));
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
            return ExecuteNonQuery((command) =>
            {
                string commandText = String.Format("update {0} set {1} where {2};", TableName,
                    UpdateParameters(command, model), FilteringParameters(command, model));

                return commandText;
            });
        }

        public virtual bool Save(TModel model)
        {
            return ExecuteNonQuery((command) =>
            {
                string commandText = String.Format("insert into {0} ({1}) values({2});", TableName,
                        ColumnNames(insertColumns), InsertParameters(command, model));

                return commandText;
            });
        }

        public virtual bool Delete(object[] primaryKeyValues)
        {
            return ExecuteNonQuery((command) =>
            {
                string commandText = String.Format("delete {0} where {1};", TableName,
                        FilteringParameters(command,primaryKeyValues));

                return commandText;
            });            
        }

        public virtual bool Overwrite(IEnumerable<TModel> models)
        {
            bool status = true;

            EntityChanges<TModel> changes = entities.EntityChanges(models, GetAll());

            changes.Added.ForEach(ent => { Save(ent); });
            changes.Deleted.ForEach(ent => { Delete(PKs(ent)); });
            changes.Updated.ForEach(ent => { Update(ent); });

            return status;
        }
    }
}