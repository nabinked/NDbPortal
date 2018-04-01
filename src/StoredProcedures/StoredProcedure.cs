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
        private readonly IDbCommand _cmd;

        public StoredProcedure(ICommandManager commandManager,
            INamingConvention namingConvention,
            IStoredProcedureSql storedProcSql,
            IOptions<DbOptions> dbOptions)
        {
            _commandManager = commandManager;
            _namingConvention = namingConvention;
            _storedProcedureSql = storedProcSql;
            _dbOptions = dbOptions.Value;
            _cmd = commandManager.GetNewCommand(CommandType.StoredProcedure);
        }
        /// <summary>
        /// Invokes a stored procedure
        /// </summary>
        /// <param name="name">name of the stored procedure</param>
        /// <param name="prms">parameters object</param>
        public void Invoke<TParams>(string name, TParams prms = null) where TParams : class
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            _commandManager.PrepareCommandForExecution(_cmd, fName, prms);
            Mapper.ExecuteNonQuery(_cmd);
        }

        public T Get<T, TParams>(TParams prm = null)
            where T : class
            where TParams : class
        {

            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            _commandManager.PrepareCommandForExecution(_cmd, GetTableInfo<T>().FullTableName, prm);
            return Mapper.GetObject<T>(_cmd);

        }

        public T Get<T, TParams>(string name, TParams prm = null)
            where T : class
            where TParams : class
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            _commandManager.PrepareCommandForExecution(_cmd, fName, prm);
            return Mapper.GetObject<T>(_cmd);
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
            _commandManager.PrepareCommandForExecution(_cmd, fName, prm);
            return Mapper.GetObjects<T>(_cmd);
        }

        public PagedList<T> GetPagedList<T, TParams>(long page, TParams prm = null)
            where T : class
            where TParams : class
        {
            return GetPagedList<T, TParams>(GetTableInfo<T>().FullTableName, page, prm);
        }

        public PagedList<T> GetPagedList<T, TParams>(string name, long page, TParams prm = null)
            where T : class
            where TParams : class
        {
            try
            {
                _cmd.CommandType = CommandType.Text;
                _commandManager.BeginTransaction(_cmd);
                var pageSize = _dbOptions.PagedListSize;
                var offset = pageSize * page;
                var sql = _storedProcedureSql.GetStoredProcQuery(name, prm);
                sql = sql.AppendLimitOffset(pageSize, offset);
                _commandManager.PrepareCommandForExecution(_cmd, sql, prm);
                //get the actual list into list property of pagedList;
                IList<T> list = Mapper.GetObjects<T>(_cmd).ToList();
                //change the command text to get the count of the query query
                var countSql = _storedProcedureSql.GetStoredProcCountQuery(name, prm);
                _commandManager.PrepareCommandForExecution(_cmd, countSql, prm);
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
