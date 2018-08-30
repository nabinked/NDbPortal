using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Options;
using NDbPortal.Extensions;
using NDbPortal.Names;

namespace NDbPortal.StoredProcedures
{
    public class StoredProcedure : IStoredProcedure
    {
        private readonly ICommandManager _commandManager;
        private readonly INamingConvention _namingConvention;
        private readonly IStoredProcedureSql _storedProcedureSql;
        private readonly DbOptions _dbOptions;

        public StoredProcedure(ICommandManager commandManager,
            INamingConvention namingConvention,
            IStoredProcedureSql storedProcSql,
            IOptions<DbOptions> dbOptions)
        {
            _commandManager = commandManager;
            _namingConvention = namingConvention;
            _storedProcedureSql = storedProcSql;
            _dbOptions = dbOptions.Value;
        }
        /// <summary>
        /// Invokes a stored procedure
        /// </summary>
        /// <param name="name">name of the stored procedure</param>
        /// <param name="prms">parameters object</param>
        public void Invoke<TParams>(string name, TParams prms = null) where TParams : class
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            var cmd = _commandManager.PrepareCommandForExecution(fName, prms, null, CommandType.StoredProcedure);
            Mapper.ExecuteNonQuery(cmd);
            Utils.Dispose(cmd);
        }

        public T Get<T, TParams>(TParams prm = null)
            where T : class
            where TParams : class
        {
            var cmd = _commandManager.PrepareCommandForExecution(GetTableInfo<T>().FullTableName, prm, null, CommandType.StoredProcedure);
            var ret = Mapper.GetObject<T>(cmd);
            Utils.Dispose(cmd);
            return ret;
        }

        public T Get<T, TParams>(string name, TParams prm = null)
            where T : class
            where TParams : class
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            var cmd = _commandManager.PrepareCommandForExecution(fName, prm, null, CommandType.StoredProcedure);
            var ret = Mapper.GetObject<T>(cmd);
            Utils.Dispose(cmd);
            return ret;
        }

        public IEnumerable<T> GetList<T, TParams>(TParams prm = null)
            where T : class
            where TParams : class
        {
            return GetList<T, TParams>(GetTableInfo<T>().TableName, prm);
        }

        public IEnumerable<T> GetList<T, TParams>(string name, TParams prm = null)
            where T : class
            where TParams : class
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            var cmd = _commandManager.PrepareCommandForExecution(fName, prm, null, CommandType.StoredProcedure);
            var ret = Mapper.GetObjects<T>(cmd);
            Utils.Dispose(cmd);
            return ret;
        }

        public IPagedList<T> GetPagedList<T, TParams>(long page, TParams prm = null)
            where T : class
            where TParams : class
        {
            return GetPagedList<T, TParams>(GetTableInfo<T>().FullTableName, page, prm);
        }

        public IPagedList<T> GetPagedList<T, TParams>(string name, long page, TParams prm = null)
            where T : class
            where TParams : class
        {
            var cmd = _commandManager.BeginTransaction();
            try
            {
                var pageSize = _dbOptions.PagedListSize;
                var offset = pageSize * page;
                var sql = _storedProcedureSql.GetStoredProcQuery(name, prm);
                sql = sql.AppendLimitOffset(pageSize, offset);
                _commandManager.PrepareCommandForExecution(sql, prm, cmd);
                //get the actual list into list property of pagedList;
                IList<T> list = Mapper.GetObjects<T>(cmd).ToList();
                //change the command text to get the count of the query query
                var countSql = _storedProcedureSql.GetStoredProcCountQuery(name, prm);
                _commandManager.PrepareCommandForExecution(countSql, prm, cmd);
                var count = Mapper.ExecuteScalar<long>(cmd);
                var pagedList = new PagedList<T>(list, count, GetCurrentPage(pageSize, offset));
                _commandManager.CommitTransaction(cmd);
                return pagedList;
            }
            catch
            {
                _commandManager.RollbackTransaction(cmd);
                throw;
            }
            finally
            {
                Utils.Dispose(cmd);
            }

        }

        #region Privates
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
