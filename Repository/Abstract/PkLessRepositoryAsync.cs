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
    public abstract class PkLessRepositoryAsync<TModel> : PkLessRepositoryBase<TModel>
        where TModel : class, ICloneable, new()
    {
        public PkLessRepositoryAsync(ConnectionStringSettings connStringSettings, string[] keyColumns, string[] identityColumns = null) :
            base(connStringSettings, keyColumns, identityColumns)
        {

        }

        public virtual async Task<List<TModel>> GetAll(params KeyValuePair<string, object>[] filterKpvs)
        {
            return await ExecuteQueryAsync<List<TModel>>((command) =>
            {
                return String.Format("select {0} from {1};", ColumnNames(columns), TableName);
            }, async (reader) =>
            {
                List<TModel> result = new List<TModel>();

                while (reader.HasRows && await reader.ReadAsync())
                {
                    result.Add(GetModelFromReader(reader));
                }

                return result;
            });
        }

        public virtual async Task<TModel> GetById(params object[] primaryKeys)
        {
            return await ExecuteQueryAsync<TModel>((command) =>
            {
                return String.Format("select {0} from {1} where {2};", ColumnNames(columns),
                    TableName, FilteringParameters(command, primaryKeys));
            }, async (reader) =>
            {
                TModel model = default(TModel);

                if (reader.HasRows)
                {
                    await reader.ReadAsync();                                      //we read ony the first row
                    model = GetModelFromReader(reader);
                }

                return model;
            });
        }

        public virtual async Task<bool> Update(TModel model)
        {
            return await ExecuteNonQueryAsync((command) =>
            {
                string commandText = String.Format("update {0} set {1} where {2};", TableName,
                    UpdateParameters(command, model), FilteringParameters(command, model));

                return commandText;
            });
        }

        public virtual async Task<bool> Save(TModel model)
        {
            return await ExecuteNonQueryAsync((command) =>
            {
                string commandText = String.Format("insert into {0} ({1}) values({2});", TableName,
                        ColumnNames(insertColumns), InsertParameters(command, model));

                return commandText;
            });
        }

        public virtual async Task<bool> Delete(object[] primaryKeyValues)
        {
            return await ExecuteNonQueryAsync((command) =>
            {
                string commandText = String.Format("delete {0} where {1};", TableName,
                        FilteringParameters(command, primaryKeyValues));

                return commandText;
            });
        }

        public virtual async Task<bool> Overwrite(IEnumerable<TModel> models)
        {
            bool status = true;

            EntityChanges<TModel> changes = entities.EntityChanges(models, await GetAll());

            changes.Added.ForEach(async ent => { await Save(ent); });
            changes.Deleted.ForEach(async ent => { await Delete(PKs(ent)); });
            changes.Updated.ForEach(async ent => { await Update(ent); });

            return status;
        }
    }
}