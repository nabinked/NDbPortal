using System.Collections.Generic;
using Microsoft.Extensions.Options;
using NDbPortal.Extensions;
using NDbPortal.Names;

namespace NDbPortal
{
    public class StoredProcedure : IStoredProcedure
    {
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;
        private readonly DbOptions _dbOptions;

        public StoredProcedure(ICommandFactory commandFactory, INamingConvention namingConvention, IOptions<DbOptions> dbOptions)
        {
            _commandFactory = commandFactory;
            _dbOptions = dbOptions.Value;
            _namingConvention = namingConvention;
        }
        /// <summary>
        /// Invokes a stored procedure
        /// </summary>
        /// <param name="name">name of the stored procedure</param>
        /// <param name="prms">parameters object</param>
        public void Invoke(string name, object prms = null)
        {
            using (var cmd = _commandFactory.Create(name, prms, true))
            {
                Mapper.ExecuteNonQuery(cmd);
            }
        }

        public T Get<T>(object prm = null)
        {

            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            using (var cmd = _commandFactory.Create(GetTableInfo<T>().FullTableName, prm, true))
            {
                return Mapper.GetObject<T>(cmd);

            }
        }

        public T Get<T>(string name, object prm = null)
        {
            using (var cmd = _commandFactory.Create(name, prm, true))
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
            using (var cmd = _commandFactory.Create(name, prm, true))
            {
                return Mapper.GetObjects<T>(cmd);
            }
        }

        public PagedList<T> GetPagedList<T>(long page, object prm = null) where T : class
        {
            return GetPagedList<T>(GetTableInfo<T>().FullTableName, page , prm);
        }

        public PagedList<T> GetPagedList<T>(string name, long page, dynamic prm = null) where T : class
        {

            var pageSize = _dbOptions.PagedListSize > 0 ? _dbOptions.PagedListSize : 10;
            var offset = pageSize * page;
            PagedList<T> pagedList;
            var sqlGenerator = new SqlGenerator(new TableInfo(name), _namingConvention);
            string sql = sqlGenerator.GetStoredProcQuery(prm);
            sql = sql.AppendLimitOffset(pageSize, offset);
            using (var cmd = _commandFactory.Create(sql, prm))
            {
                //get the actual list into list property of pagedList;
                IList<T> list = Mapper.GetObjects<T>(cmd, false);
                //change the command text to get the count of the query query
                cmd.CommandText = sqlGenerator.GetStoredProcCountQuery(prm);
                pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(cmd), GetCurrentPage(pageSize, offset), pageSize)
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
            return new TableInfoBuilder<T>(_namingConvention)
                            .SetColumnInfos()
                            .SetPrimaryKey()
                            .SetTableName()
                            .Build();
        }
        #endregion

    }
}
