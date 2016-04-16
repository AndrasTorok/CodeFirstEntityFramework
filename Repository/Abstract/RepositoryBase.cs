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
    public abstract class RepositoryBase<TModel>
        where TModel : class, ICloneable, new()
    {
        private const string cMultipleActiveResultSets = "MultipleActiveResultSets";
        private const string cSqlServerProvider = "System.Data.SqlClient";
        protected readonly DbProviderFactory factory;
        protected readonly ConnectionStringSettings connStringSettings;
        protected readonly List<string> columns;
        protected readonly List<string> insertColumns;

        public RepositoryBase(ConnectionStringSettings connStringSettings, string[] identityColumns = null)
        {
            this.connStringSettings = connStringSettings;
            if (connStringSettings.ProviderName == cSqlServerProvider && false)
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connStringSettings.ConnectionString;
                if (!builder.ContainsKey(cMultipleActiveResultSets))
                {
                    builder.Add(cMultipleActiveResultSets, true);
                    this.connStringSettings.ConnectionString = builder.ToString();
                }
            }
            this.factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            this.columns = typeof(TModel).GetProperties().Where(p => !p.GetMethod.IsVirtual).Select(m => m.Name).ToList();
            this.insertColumns = this.columns.Except(identityColumns != null ? identityColumns : new string[0]).ToList();
        }         

        protected abstract string TableName { get; }        

        #region Execute Query

        protected TReturn ExecuteQuery<TReturn>(Func<DbCommand, string> commandFct, Func<DbDataReader, TReturn> fct)
        {
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = commandFct(command);

                    connection.Open();
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        return fct(reader);
                    }
                }
            }
        }

        protected async Task<TReturn> ExecuteQueryAsync<TReturn>(Func<DbCommand, string> commandFct, Func<DbDataReader, Task<TReturn>> fct)
        {
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = commandFct(command);

                    await connection.OpenAsync();
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        return await fct(reader);
                    }
                }
            }
        }

        #endregion

        #region Execute Non Query

        protected bool ExecuteNonQuery(Func<DbCommand,string> commandFct)
        {
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = commandFct(command);

                    connection.Open();
                    int count = command.ExecuteNonQuery();
                    bool status = count > 0;

                    return status;
                }
            }
        }

        protected async Task<bool> ExecuteNonQueryAsync(Func<DbCommand, string> commandFct)
        {
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connStringSettings.ConnectionString;
                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = commandFct(command);

                    await connection.OpenAsync();
                    int count = await command.ExecuteNonQueryAsync();
                    bool status = count > 0;

                    return status;
                }
            }
        }

        #endregion              

        #region Parameters        

        protected string UpdateParameters(DbCommand command, TModel model)
        {
            return Parameters(command, model, insertColumns, "param", (index, key, paramName) =>
            {
                return String.Format("{0}{1} = {2}", index > 0 ? ", " : "", key, paramName);
            });
        }

        protected string InsertParameters(DbCommand command,TModel model)
        {            
            return Parameters(command, model, insertColumns, "param", (index, key, paramName) =>
            {
                return String.Format("{0}{1}", index > 0 ? ", " : "", paramName);
            });
        }

        protected string Parameters(DbCommand command, TModel model, List<string> columns, string paramName, 
            Func<int, string, string, string> paramFormat, object[] values = null)
        {
            if (model == default(TModel) && values == null) throw new ApplicationException("Parameters error: either model or values should be provided");
            StringBuilder sb = new StringBuilder();

            for (var index = 0; index < columns.Count(); index++)
            {
                string key = columns[index];
                object value = values != null ? values[index] : GetValue(model, columns[index]);
                if (value == null) value = DBNull.Value;

                DbParameter parameter = factory.CreateParameter();
                parameter.ParameterName = String.Format("@{0}{1}",paramName, index);
                parameter.Value = value;
                command.Parameters.Add(parameter);
                sb.Append(paramFormat(index, key, parameter.ParameterName));
            }

            return sb.ToString();
        }        

        #endregion

        #region Helpers

        protected string ColumnNames(List<string> columns)
        {
            StringBuilder sb = new StringBuilder();

            for (var i = 0; i < columns.Count; i++)
            {
                sb.AppendFormat("{0}{1}", i > 0 ? ", " : "", columns[i]);
            }

            return sb.ToString();
        }

        protected object GetValue(TModel model, string key)
        {
            Type type = typeof(TModel);
            object value = type.GetProperty(key).GetValue(model, null);

            return value;
        }

        protected List<string> PropertiesWithValues(TModel model)
        {
            Type type = typeof(TModel);

            List<string> propetiesWithValues =
                (from p in type.GetProperties()
                 let val = p.GetValue(model)
                 let defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null
                 where val != defaultValue
                 select p.Name).ToList();

            return propetiesWithValues;
        }

        protected TModel GetModelFromReader(DbDataReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();     //dictionary with model properties
            
            for (int index = 0; index < columns.Count; index++)                     //loop through  the properties of the model
            {
                dict.Add(columns[index], reader.GetValue(index));                   //add the values obtained from the reader to the dict
            }

            string json = JsonConvert.SerializeObject(dict);                        //serialize the dictionary to Json
            TModel model = JsonConvert.DeserializeObject<TModel>(json);             //deserialize the Json to the model itself

            return model;
        }        

        #endregion
    }
}