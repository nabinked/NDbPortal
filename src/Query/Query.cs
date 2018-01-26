using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using NDbPortal.Names;

namespace NDbPortal.Query
{
    public class Query<T> : IQuery<T> where T : class
    {
        private readonly ICommandManager _commandManager;
        private readonly DbOptions _dbOptions;
        private readonly ISqlGenerator<T> _sqlGenerator;
        private readonly IDbCommand _cmd;

        public Query(IOptions<DbOptions> dbOptions, ICommandManager commandManager, ISqlGenerator<T> sqlGenerator)
        {
            _commandManager = commandManager;
            _sqlGenerator = sqlGenerator;
            _dbOptions = dbOptions.Value;
            _cmd = commandManager.GetCommand();
        }
        public T Get(long id)
        {
            using (var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectByIdQuery(), new { id }))
            {
                return Mapper.GetObject<T>(cmd);
            }
        }

        public IEnumerable<T> GetAll()
        {
            using (var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectAllQuery()))
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
            var columnName = _sqlGenerator.NamingConvention.ConvertToDbName(propertyName);
            using (var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectByColumnNameQuery(columnName), obj))
            {
                return Mapper.GetObject<T>(cmd);
            }

        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            var columnName = _sqlGenerator.NamingConvention.ConvertToDbName(propertyName);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[propertyName] = ReflectionUtilities.GetValueFromExpression(expression);
            using (var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectByColumnNameQuery(columnName), obj))
            {
                return Mapper.GetObjects<T>(cmd);
            }
        }

        public PagedList<T> GetPagedList(int page, string orderBy = "id")
        {
            var sql = _sqlGenerator.GetPagedQuery(page, orderBy);
            PagedList<T> pagedList;
            using (var cmd = _commandManager.PrepareCommandForExecution(sql))
            {
                IList<T> list = Mapper.GetObjects<T>(cmd).ToList();
                cmd.CommandText = _sqlGenerator.GetCountQuery();
                pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(cmd), page)
                {
                    List = list
                };
            }
            return pagedList;

        }

    }
}