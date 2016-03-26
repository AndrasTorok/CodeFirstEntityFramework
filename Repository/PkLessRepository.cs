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
    public abstract class PkLessRepository<TEntity, TModel>
        where TEntity : class, new()
        where TModel : class, new()
    {
        private readonly DbContext dataContext;
        private readonly ConnectionStringSettings connStringSettings;
        private readonly List<string> keyColumns;
        private readonly List<string> valueColumns;
        private readonly List<string> columns;
        private readonly List<string> insertColumns;

        public PkLessRepository(ConnectionStringSettings connStringSettings, string[] keyColumns, string[] identityColumns = null)
        {
            this.connStringSettings = connStringSettings;
            this.dataContext = new DbContext(connStringSettings.ConnectionString);
            this.keyColumns = keyColumns.ToList();
            this.valueColumns = typeof(TEntity).GetProperties().Select(m => m.Name).Except(this.keyColumns).ToList();
            this.columns = this.keyColumns.Union(this.valueColumns).ToList();
            this.insertColumns = this.columns.Except(identityColumns != null ? identityColumns : new string[0]).ToList();
        }

        public abstract TModel ModelFromEntity(TEntity entity);
        public abstract TEntity EntityFromModel(TModel model);
        public abstract string TableName { get; }

        public virtual IEnumerable<TModel> GetAll()
        {
            return dataContext.Set<TEntity>().ToList().Select(e => ModelFromEntity(e)).ToList();
        }

        public virtual bool Update(TModel model)
        {
            bool status = false;

            DbProviderFactory factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    string filterParametersText = FilteringParameters(factory, command, model);
                    string updateParametersText = UpdateParameters(factory, command, model);

                    command.Connection = connection;
                    command.CommandText = String.Format("update {0} set {1} where {2};", TableName, updateParametersText, filterParametersText);

                    connection.Open();
                    int count = command.ExecuteNonQuery();
                    status = count > 0;
                }
            }

            return status;
        }

        public virtual bool Save(TModel model)
        {
            bool status = false;

            DbProviderFactory factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    string insertParametersText = InsertParameters(factory, command, model);

                    command.Connection = connection;
                    command.CommandText = String.Format("insert into {0} ({1}) values({2});", TableName, ColumnNamesForInsert(), insertParametersText);

                    connection.Open();
                    int count = command.ExecuteNonQuery();
                    status = count > 0;
                }
            }

            return status;
        }

        public virtual bool Delete(object[] primaryKeyValues)
        {
            bool status = false;

            DbProviderFactory factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    string filterParametersText = FilteringParameters(factory, command, primaryKeyValues);

                    command.Connection = connection;
                    command.CommandText = String.Format("delete {0} where {1};", TableName, filterParametersText);

                    connection.Open();
                    int count = command.ExecuteNonQuery();
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

        private string ColumnNamesForInsert()
        {
            StringBuilder sb = new StringBuilder();

            for (var i = 0; i < insertColumns.Count; i++)
            {
                sb.AppendFormat("{0}{1}", i > 0 ? ", " : "", insertColumns[i]);
            }

                return sb.ToString();
        }

        #endregion

        #region Reflection

        private object GetValue(TModel model, string key)
        {
            Type type = typeof(TModel);
            object value = type.GetProperty(key).GetValue(model, null);

            return value;
        }

        #endregion
    }
}
