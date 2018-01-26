using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Options;
using NDbPortal.Extensions;
using NDbPortal.Names;

namespace NDbPortal
{
    public class StoredProcedure : IStoredProcedure
    {
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandBuilder _commandBuilder;
        private readonly INamingConvention _namingConvention;
        private readonly DbOptions _dbOptions;
        private IDbCommand _cmd;

        public StoredProcedure(ICommandFactory commandFactory, ICommandBuilder commandBuilder, INamingConvention namingConvention, IOptions<DbOptions> dbOptions)
        {
            _commandFactory = commandFactory;
            _commandBuilder = commandBuilder;
            _cmd = commandFactory.Create(true);
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
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            using (var cmd = _commandBuilder.GetFinalCommand(_cmd, fName, prms))
            {
                Mapper.ExecuteNonQuery(cmd);
            }
        }


        public T Get<T>(object prm = null)
        {

            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            using (var cmd = _commandBuilder.GetFinalCommand(_cmd, GetTableInfo<T>().FullTableName, prm))
            {
                return Mapper.GetObject<T>(cmd);

            }
        }

        public T Get<T>(string name, object prm = null)
        {
            var fName = Utils.GetSchemaQualifiedName(name, _dbOptions.DefaultSchema);
            using (var cmd = _commandBuilder.GetFinalCommand(_cmd, fName, prm))
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
            using (var cmd = _commandBuilder.GetFinalCommand(_cmd, fName, prm))
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
            var sqlGenerator = new SqlGenerator(name, _dbOptions.DefaultSchema, _namingConvention);
            string sql = sqlGenerator.GetStoredProcQuery(prm);
            sql = sql.AppendLimitOffset(pageSize, offset);
            using (var cmd = _commandBuilder.GetFinalCommand(_cmd, sql, prm))
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
            return new TableInfoBuilder<T>(_namingConvention, _dbOptions)
                            .SetColumnInfos()
                            .SetPrimaryKey()
                            .SetTableName()
                            .Build();
        }
        #endregion

    }
}
