using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using NDbPortal.Names;

namespace NDbPortal
{
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        public ITableInfoBuilder<T> TableInfoBuilder { get; set; }
        private readonly ICommandFactory _commandFactory;
        private short PageSize { get; set; }
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
            SqlGenerator = new SqlGenerator(TableInfoBuilder.Build(), namingConvention);
            PageSize = (short)(dbOptions.Value.PagedListSize > 0 ? dbOptions.Value.PagedListSize : 10);
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

        public long Update(T obj)
        {
            using (var cmd = _commandFactory.Create(SqlGenerator.GetUpdateQuery(), obj))
            {
                return Mapper.GetObject<long>(cmd);

            }
        }

        public bool Remove(long id)
        {
            using (var cmd = _commandFactory.Create(SqlGenerator.GetDeleteQuery(), new { id }))
            {
                return Mapper.ExecuteNonQuery(cmd) > 0;

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
            PagedList<T> pagedList;
            using (var cmd = _commandFactory.Create(sql, new { limit = PageSize, offset = GetOffset(page) }))
            {
                IList<T> list = Mapper.GetObjects<T>(cmd, false).ToList();
                cmd.CommandText = SqlGenerator.GetCountQuery();
                pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(cmd), page, PageSize)
                {
                    List = list
                };
            }
            return pagedList;

        }

        #region Privates

        private long GetOffset(long page)
        {
            return page * PageSize;
        }

        /// <summary>
        /// Checks if the object primary key has any value to determine if its new.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool IsNew(T t)
        {
            var primaryKey = ReflectionUtilities.GetPrimaryKey(typeof(T).GetTypeInfo());
            if (t.HasProperty(primaryKey))
            {
                var primaryKeyProperty =
                    t.GetType()
                        .GetProperties()
                        .FirstOrDefault(x => x.Name.Equals(primaryKey, StringComparison.OrdinalIgnoreCase));
                var primaryKeyValue = primaryKeyProperty.GetGetMethod().Invoke(t, null);

                if (primaryKeyValue == null ||
                    primaryKeyValue.Equals(Activator.CreateInstance(primaryKeyProperty.PropertyType)))
                {
                    return true;
                }
            }
            return false;
        }

        public long Upsert(T obj)
        {
            if (IsNew(obj))
            {
                return Add(obj);
            }
            else
            {
                return Update(obj);
            }
        }

        #endregion

    }
}
