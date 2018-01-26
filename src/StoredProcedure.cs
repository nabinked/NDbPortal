using System.Collections.Generic;
using System.Data;
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

        public StoredProcedure(ICommandManager commandManager, INamingConvention namingConvention, IOptions<DbOptions> dbOptions)
        {
            _commandManager = commandManager;
            _namingConvention = namingConvention;
            _dbOptions = dbOptions.Value;
        }
        /// <summary>
        /// Invokes a stored procedure
        /// </summary>
        /// <param name="name">name of the stored procedure</param>
        /// <param name="prms">parameters object</param>
        public void Invoke(string name, object prms = null)
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            using (var cmd = _commandManager.PrepareCommandForExecution(fName, prms))
            {
                Mapper.ExecuteNonQuery(cmd);
            }
        }

        public T Get<T>(object prm = null)
        {

            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            using (var cmd = _commandManager.PrepareCommandForExecution(GetTableInfo<T>().FullTableName, prm))
            {
                return Mapper.GetObject<T>(cmd);

            }
        }

        public T Get<T>(string name, object prm = null)
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            using (var cmd = _commandManager.PrepareCommandForExecution(fName, prm))
            {
                return Mapper.GetObject<T>(cmd);
            }
        }

        public IEnumerable<T> GetList<T>(object prm = null)
        {
            return GetList<T>(GetTableInfo<T>().TableName, prm);
        }

        public IEnumerable<T> GetList<T>(string name, object prm = null)
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            using (var cmd = _commandManager.PrepareCommandForExecution(fName, prm))
            {
                return Mapper.GetObjects<T>(cmd);
            }
        }

        public PagedList<T> GetPagedList<T>(long page, object prm = null) where T : class
        {
            return GetPagedList<T>(GetTableInfo<T>().FullTableName, page, prm);
        }

        public PagedList<T> GetPagedList<T>(string name, long page, dynamic prm = null) where T : class
        {
            var pageSize = _dbOptions.PagedListSize;
            var offset = pageSize * page;
            PagedList<T> pagedList;
            var sqlGenerator = new SqlGenerator<T>(_namingConvention, _dbOptions);
            string sql = sqlGenerator.GetStoredProcQuery(prm);
            sql = sql.AppendLimitOffset(pageSize, offset);
            using (var cmd = _commandManager.PrepareCommandForExecution(sql, prm))
            {
                //get the actual list into list property of pagedList;
                IList<T> list = Mapper.GetObjects<T>(cmd);
                //change the command text to get the count of the query query
                cmd.CommandText = sqlGenerator.GetStoredProcCountQuery(prm);
                pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(cmd), GetCurrentPage(pageSize, offset))
                {
                    List = list
                };
            }
            return pagedList;


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
