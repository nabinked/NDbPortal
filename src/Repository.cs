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
        private readonly IConnectionFactory _connectionFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;
        private readonly ICommandBuilder _cmdBuilder;
        private readonly DbOptions _dbOptions;
        private readonly SqlGenerator _sqlGenerator;
        private readonly short _pageSize;


        public Repository(IConnectionFactory connectionFactory, ICommandFactory commandFactory,
                            INamingConvention namingConvention, IOptions<DbOptions> dbOptions,
                            ICommandBuilder cmdBuilder)
        {
            _namingConvention = namingConvention;
            _cmdBuilder = cmdBuilder;
            _dbOptions = dbOptions.Value;
            _connectionFactory = connectionFactory;
            _commandFactory = commandFactory;
            var tableInfoBuilder = GetTableInfoBuilder();
            _sqlGenerator = new SqlGenerator(tableInfoBuilder.Build(), namingConvention);
            _pageSize = (short)(dbOptions.Value.PagedListSize > 0 ? dbOptions.Value.PagedListSize : 10);
        }

        private ITableInfoBuilder<T> GetTableInfoBuilder()
        {
            return new TableInfoBuilder<T>(_namingConvention, _dbOptions)
                .SetTableName()
                .SetPrimaryKey()
                .SetColumnInfos();
        }


        public T Get(long id)
        {
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetSelectByIdQuery(), new { id }))
            {
                return Mapper.GetObject<T>(cmd);
            }
        }

        public IEnumerable<T> GetAll()
        {
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetSelectAllQuery()))
            {
                return Mapper.GetObjects<T>(cmd);
            }

        }

        public long Add(T obj)
        {
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetInsertQuery(), obj))
            {
                return Mapper.GetObject<long>(cmd);

            }
        }

        public bool AddRange(IEnumerable<T> entities)
        {
            using (var conn = _connectionFactory.Create())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                var cmd = conn.CreateCommand();
                foreach (T entity in entities)
                {
                    cmd = _cmdBuilder.GetFinalCommand(cmd, _sqlGenerator.GetInsertQuery(), entity);
                    Mapper.GetObject<long>(cmd, false);
                }
                transaction.Commit();
            }
            return true;
        }

        public long Update(T obj)
        {
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetUpdateQuery(), obj))
            {
                return Mapper.GetObject<long>(cmd);
            }
        }

        public bool Remove(long id)
        {
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetDeleteQuery(), new { id }))
            {
                return Mapper.ExecuteNonQuery(cmd) > 0;
            }
        }

        public T Find(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[propertyName] = ReflectionUtilities.GetValueFromExpression(expression);
            var columnName = _namingConvention.ConvertToDbName(propertyName);
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetSelectByColumnNameQuery(columnName), obj))
            {
                return Mapper.GetObject<T>(cmd);
            }

        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            var columnName = _namingConvention.ConvertToDbName(propertyName);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[propertyName] = ReflectionUtilities.GetValueFromExpression(expression);
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetSelectByColumnNameQuery(columnName), obj))
            {
                return Mapper.GetObjects<T>(cmd);
            }
        }

        public PagedList<T> GetPagedList(long page, string orderBy = "id")
        {
            var sql = _sqlGenerator.GetPagedQuery(orderBy);
            PagedList<T> pagedList;
            using (var cmd = _commandFactory.Create(sql, new { limit = _pageSize, offset = GetOffset(page) }))
            {
                IList<T> list = Mapper.GetObjects<T>(cmd, false).ToList();
                cmd.CommandText = _sqlGenerator.GetCountQuery();
                pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(cmd), page, _pageSize)
                {
                    List = list
                };
            }
            return pagedList;

        }

        #region Privates

        private long GetOffset(long page)
        {
            return page * _pageSize;
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
