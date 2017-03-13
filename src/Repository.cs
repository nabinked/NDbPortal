using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using NDbPortal.Names;

namespace NDbPortal
{
    public class Repository<T> : IRepository<T> where T : class, new()
    {


        public ITableInfoBuilder<T> TableInfoBuilder { get; set; }
        private readonly ICommandFactory _commandFactory;
        public short PageSize { get; set; }
        public INamingConvention NamingConvention { get; set; }

        public Repository(ICommandFactory commandFactory, INamingConvention namingConvention, IOptions<DbOptions> dbOptions)
        {
            NamingConvention = namingConvention ?? new NamingConvention()
            {
                DbNamingConvention = DbEnums.NamingConventions.UnderScoreCase,
                PocoNamingConvention = DbEnums.NamingConventions.PascalCase
            };
            _commandFactory = commandFactory;
            TableInfoBuilder = new TableInfoBuilder<T>(NamingConvention)
                .SetTableName()
                .SetPrimaryKey()
                .SetColumnInfos();
            SqlGenerator = new SqlGenerator(TableInfoBuilder.Build());
            PageSize = (short)(dbOptions.Value.PagedListSize > 0 ? dbOptions.Value.PagedListSize : 20);
        }

        public SqlGenerator SqlGenerator { get; set; }


        public T Get(long id)
        {
            using (var cmd = _commandFactory.Create(SqlGenerator.GetSelectByIdQuery(), new { id }))
            {
                return Mapper.GetObject<T>(cmd);
            }

        }

        public IEnumerable<T> GetAll()
        {
            using (var cmd = _commandFactory.Create(SqlGenerator.GetSelectAllQuery()))
            {
                return Mapper.GetObjects<T>(cmd);

            }

        }

        public long Add(T obj)
        {
            using (var cmd = _commandFactory.Create(SqlGenerator.GetInsertQuery(), obj))
            {
                return Mapper.GetObject<long>(cmd);

            }
        }

        public bool AddRange(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                Add(entity);
            }
            return true;
        }

        public bool Update(T obj)
        {
            using (var cmd = _commandFactory.Create(SqlGenerator.GetUpdateQuery(), obj))
            {
                return Mapper.GetObject<long>(cmd) > 0;

            }
        }

        public bool Remove(long id)
        {
            using (var cmd = _commandFactory.Create(SqlGenerator.GetDeleteQuery(), new { id }))
            {
                return cmd.ExecuteNonQuery() > 0;

            }
        }

        public T Find(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            var columnName = NamingConvention.ConvertToDbName(propertyName);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[columnName] = ReflectionUtilities.GetValueFromExpression(expression);

            using (var cmd = _commandFactory.Create(SqlGenerator.GetSelectByColumnNameQuery(columnName), obj))
            {
                return Mapper.GetObject<T>(cmd);
            }

        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            var columnName = NamingConvention.ConvertToDbName(propertyName);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[columnName] = ReflectionUtilities.GetValueFromExpression(expression);
            using (var cmd = _commandFactory.Create(SqlGenerator.GetSelectByColumnNameQuery(columnName), obj))
            {
                return Mapper.GetObjects<T>(cmd);
            }
        }

        public PagedList<T> GetPagedList(long page, string orderBy = "id")
        {
            var sql = SqlGenerator.GetPagedQuery(orderBy);
            IList<T> list;
            using (var cmd = _commandFactory.Create(sql, new { limit = PageSize, offset = GetOffset(page) }))
            {
                list = Mapper.GetObjects<T>(cmd).ToList();
            }
            using (var cmd = _commandFactory.Create(SqlGenerator.GetCountQuery()))
            {
                var pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(cmd),page,PageSize)
                {
                    List = list
                };
                return pagedList;
            }
        }

        #region Privates

        private long GetOffset(long page)
        {
            return page * PageSize;
        }

        #endregion

    }
}
