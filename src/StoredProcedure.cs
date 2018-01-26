using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Options;
using NDbPortal.Extensions;
using NDbPortal.Names;

namespace NDbPortal
{
    public class StoredProcedure : IStoredProcedure
    {
        private readonly ICommandManager _commandManager;
        private readonly INamingConvention _namingConvention;
        private readonly DbOptions _dbOptions;
        private IDbCommand _cmd;

        public StoredProcedure(ICommandManager commandManager, INamingConvention namingConvention, IOptions<DbOptions> dbOptions)
        {
            _commandManager = commandManager;
            _namingConvention = namingConvention;
            _dbOptions = dbOptions.Value;
            _cmd = commandManager.GetNewCommand();
        }
        /// <summary>
        /// Invokes a stored procedure
        /// </summary>
        /// <param name="name">name of the stored procedure</param>
        /// <param name="prms">parameters object</param>
        public void Invoke(string name, object prms = null)
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            _commandManager.PrepareCommandForExecution(_cmd, fName, prms);
            Mapper.ExecuteNonQuery(_cmd);
        }

        public T Get<T>(object prm = null)
        {

            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            _commandManager.PrepareCommandForExecution(_cmd, GetTableInfo<T>().FullTableName, prm);
            return Mapper.GetObject<T>(_cmd);

        }

        public T Get<T>(string name, object prm = null)
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            _commandManager.PrepareCommandForExecution(_cmd, fName, prm);
            return Mapper.GetObject<T>(_cmd);
        }

        public IEnumerable<T> GetList<T>(object prm = null)
        {
            return GetList<T>(GetTableInfo<T>().TableName, prm);
        }

        public IEnumerable<T> GetList<T>(string name, object prm = null)
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            _commandManager.PrepareCommandForExecution(_cmd, fName, prm);
            return Mapper.GetObjects<T>(_cmd);
        }

        public PagedList<T> GetPagedList<T>(long page, object prm = null) where T : class
        {
            return GetPagedList<T>(GetTableInfo<T>().FullTableName, page, prm);
        }

        public PagedList<T> GetPagedList<T>(string name, long page, dynamic prm = null) where T : class
        {
            try
            {
                _commandManager.BeginTransaction(_cmd);
                var pageSize = _dbOptions.PagedListSize;
                var offset = pageSize * page;
                var sqlGenerator = new SqlGenerator<T>(_namingConvention, _dbOptions);
                string sql = sqlGenerator.GetStoredProcQuery(prm);
                sql = sql.AppendLimitOffset(pageSize, offset);
                _commandManager.PrepareCommandForExecution(_cmd, sql, prm);
                //get the actual list into list property of pagedList;
                IList<T> list = Mapper.GetObjects<T>(_cmd).ToList();
                //change the command text to get the count of the query query
                _commandManager.PrepareCommandForExecution(_cmd, sqlGenerator.GetStoredProcCountQuery(prm));
                var pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(_cmd), GetCurrentPage(pageSize, offset))
                {
                    List = list
                };
                _commandManager.CommitTransaction(_cmd);
                return pagedList;
            }
            catch
            {
                _commandManager.RollbackTransaction(_cmd);
                throw;
            }

        }

        private long GetCurrentPage(int? pageSize, long skip)
        {
            if (pageSize == null)
            {
                return 0;
            }
            else
            {
                return skip / pageSize.Value;
            }
        }

        #region Privates
        private TableInfo GetTableInfo<T>()
        {
            return new TableInfoBuilder<T>(_namingConvention, _dbOptions)
                            .SetColumnInfos()
                            .SetPrimaryKey()
                            .SetTableName()
                            .Build();
        }
        #endregion

    }
}
