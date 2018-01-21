using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using NDbPortal.Names;

namespace NDbPortal.Query
{
    class Query<T> : IQuery<T> where T : class
    {
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;
        private ICommandBuilder _cmdBuilder;
        private readonly DbOptions _dbOptions;
        private IConnectionFactory _connectionFactory;
        private readonly SqlGenerator _sqlGenerator;
        private readonly short _pageSize;

        public Query(IConnectionFactory connectionFactory, ICommandFactory commandFactory,
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

        private ITableInfoBuilder<T> GetTableInfoBuilder()
        {
            return new TableInfoBuilder<T>(_namingConvention, _dbOptions)
                .SetTableName()
                .SetPrimaryKey()
                .SetColumnInfos();
        }


        #endregion


    }
}