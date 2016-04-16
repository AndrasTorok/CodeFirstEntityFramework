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
    public abstract class PkLessRepositoryBase<TModel> : RepositoryBase<TModel>
        where TModel : class, ICloneable, new()
    {
        protected readonly List<string> keyColumns;
        protected readonly List<string> valueColumns;
        protected Entities<TModel> entities;

        public PkLessRepositoryBase(ConnectionStringSettings connStringSettings, string[] keyColumns, string[] identityColumns = null)
            : base(connStringSettings, identityColumns)
        {            
            this.keyColumns = keyColumns.ToList();
            this.valueColumns = typeof(TModel).GetProperties().Where(p => !p.GetMethod.IsVirtual).Select(m => m.Name).Except(this.keyColumns).ToList();
            this.entities = new Entities<TModel>(this.keyColumns);       
        }         

        public object[] PKs(TModel model)
        {
            return this.keyColumns.Select(key => GetValue(model, key)).ToArray();
        }        

        #region Parameters

        protected string FilteringParameters(DbCommand command, TModel model)
        {
            object[] primaryKeyValues = this.PKs(model);
            return FilteringParameters(command, primaryKeyValues);
        }

        protected string FilteringParameters(DbCommand command, object[] primaryKeyValues)
        {
            if (this.keyColumns.Count() != primaryKeyValues.Count()) throw new ApplicationException("Mismatch in PK");            

            return Parameters(command, null, keyColumns, "key", (index, key, paramName) =>
            {
                return String.Format("{0}{1} = {2}", index > 0 ? " and " : "", key, paramName);
            }, primaryKeyValues);
        }                       

        #endregion        
    }
}