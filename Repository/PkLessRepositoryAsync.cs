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
    public abstract class PkLessRepositoryAsync<TEntity, TModel>
        where TEntity : class, new()
        where TModel : class, new()
    {
        private readonly DbContext dataContext;
        private readonly ConnectionStringSettings connStringSettings;
        private readonly List<string> keyColumns;
        private readonly List<string> valueColumns;
        private readonly List<string> columns;
        private readonly List<string> insertColumns;
        private Dictionary<string, string> cols;

        public PkLessRepositoryAsync(ConnectionStringSettings connStringSettings, string[] keyColumns, string[] identityColumns = null)
        {
            this.connStringSettings = connStringSettings;
            this.dataContext = new EFContext();
            this.keyColumns = keyColumns.ToList();
            this.valueColumns = typeof(TEntity).GetProperties().Select(m => m.Name).Except(this.keyColumns).ToList();
            this.columns = this.keyColumns.Union(this.valueColumns).ToList();
            this.insertColumns = this.columns.Except(identityColumns != null ? identityColumns : new string[0]).ToList();
            FillColumnMapping();
        }

        public abstract TModel ModelFromEntity(TEntity entity);
        public abstract TEntity EntityFromModel(TModel model);
        public abstract string TableName { get; }

        public virtual IEnumerable<TModel> GetAll()
        {
            return dataContext.Set<TEntity>().ToList().Select(e => ModelFromEntity(e)).ToList();
        }

        public virtual async Task<TModel> GetById(params object[] primaryKeys)
        {
            TModel model = default(TModel);

            DbProviderFactory factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = String.Format("select {0} from {1} where {2};", ColumnNames(columns), TableName,
                        FilteringParameters(factory, command, primaryKeys));

                    await connection.OpenAsync();
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();                                      //we read ony the first row
                            model = GetModelFromReader(reader);
                        }
                    }
                }
            }

            return model;
        }

        public virtual async Task<bool> Update(TModel model)
        {
            bool status = false;

            DbProviderFactory factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = String.Format("update {0} set {1} where {2};", TableName,
                        UpdateParameters(factory, command, model), FilteringParameters(factory, command, model));

                    await connection.OpenAsync();
                    int count = await command.ExecuteNonQueryAsync();
                    status = count > 0;
                }
            }

            return status;
        }

        public virtual async Task<bool> Save(TModel model)
        {
            bool status = false;

            DbProviderFactory factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = String.Format("insert into {0} ({1}) values({2});", TableName,
                        ColumnNames(insertColumns), InsertParameters(factory, command, model));

                    await connection.OpenAsync();
                    int count = await command.ExecuteNonQueryAsync();
                    status = count > 0;
                }
            }

            return status;
        }

        public virtual async Task<bool> Delete(object[] primaryKeyValues)
        {
            bool status = false;

            DbProviderFactory factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = String.Format("delete {0} where {1};", TableName,
                        FilteringParameters(factory, command, primaryKeyValues));

                    await connection.OpenAsync();
                    int count = await command.ExecuteNonQueryAsync();
                    status = count > 0;
                }
            }

            return status;
        }

        #region Parameters

        private string FilteringParameters(DbProviderFactory factory, DbCommand command, TModel model)
        {
            object[] primaryKeyValues = this.keyColumns.Select(key => GetValue(model, key)).ToArray();
            return FilteringParameters(factory, command, primaryKeyValues);
        }

        private string FilteringParameters(DbProviderFactory factory, DbCommand command, object[] primaryKeyValues)
        {
            if (this.keyColumns.Count() != primaryKeyValues.Count()) throw new ApplicationException("Mismatch in PK");
            StringBuilder sb = new StringBuilder();

            for (var index = 0; index < primaryKeyValues.Count(); index++)
            {
                string key = this.keyColumns[index];
                DbParameter parameter = factory.CreateParameter();
                parameter.ParameterName = String.Format("@key{0}", index);
                parameter.Value = primaryKeyValues[index];
                command.Parameters.Add(parameter);
                sb.AppendFormat("{0}{1} = {2}", index > 0 ? " and " : "", key, parameter.ParameterName);
            }

            return sb.ToString();
        }

        private string UpdateParameters(DbProviderFactory factory, DbCommand command, TModel model)
        {
            StringBuilder sb = new StringBuilder();

            for (var index = 0; index < valueColumns.Count(); index++)
            {
                string key = valueColumns[index];
                DbParameter parameter = factory.CreateParameter();
                parameter.ParameterName = String.Format("@param{0}", index);
                parameter.Value = GetValue(model, valueColumns[index]);
                command.Parameters.Add(parameter);
                sb.AppendFormat("{0}{1} = {2}", index > 0 ? ", " : "", key, parameter.ParameterName);
            }

            return sb.ToString();
        }

        private string InsertParameters(DbProviderFactory factory, DbCommand command, TModel model)
        {
            StringBuilder sb = new StringBuilder();

            for (var index = 0; index < insertColumns.Count(); index++)
            {
                string key = insertColumns[index];
                DbParameter parameter = factory.CreateParameter();
                parameter.ParameterName = String.Format("@param{0}", index);
                parameter.Value = GetValue(model, insertColumns[index]);
                command.Parameters.Add(parameter);
                sb.AppendFormat("{0}{1}", index > 0 ? ", " : "", parameter.ParameterName);
            }

            return sb.ToString();
        }        

        private string ColumnNames(List<string> columns)
        {
            StringBuilder sb = new StringBuilder();

            for (var i = 0; i < columns.Count; i++)
            {
                sb.AppendFormat("{0}{1}", i > 0 ? ", " : "", columns[i]);
            }

            return sb.ToString();
        }

        #endregion

        #region Helpers

        private object GetValue(TModel model, string key)
        {
            Type type = typeof(TModel);
            object value = type.GetProperty(key).GetValue(model, null);

            return value;
        }

        private TModel GetModelFromReader(DbDataReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int index = 0; index < columns.Count; index++)
            {
                dict.Add(columns[index], reader.GetValue(index));
            }

            string json = JsonConvert.SerializeObject(dict);
            TModel model = JsonConvert.DeserializeObject<TModel>(json);

            return model;
        }

        private void FillColumnMapping()
        {
            Type entityType = typeof(TEntity),
                modelType = typeof(TModel);

            List<string> entityCols = entityType.GetProperties().Select(c => c.Name).ToList();
            List<string> modelCols = modelType.GetProperties().Select(c => c.Name).ToList();

            Dictionary<string, string> containedColumns = entityCols.Where(ec => modelCols.Contains(ec)).ToDictionary(ec => ec, ec => ec),
                notContainedColumns = (from ec in entityCols
                                       where !modelCols.Contains(ec)
                                       join attr in modelType.GetCustomAttributes(false).OfType<ColumnNameMappingAttribute>() on ec equals attr.EntityName
                                       select new
                                       {
                                           EntityName = ec,
                                           ModelName = attr.ModelName
                                       }).ToDictionary(n => n.EntityName, n => n.ModelName);

            cols = containedColumns.Union(notContainedColumns).ToDictionary(c => c.Key, c => c.Value);
        }

        #endregion
    }
}