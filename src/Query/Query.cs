using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using NDbPortal.Names;

namespace NDbPortal.Query
{
    public class Query<T, TKey> : IQuery<T, TKey> where T : class
    {
        private readonly ICommandManager _commandManager;
        private readonly ISqlGenerator<T> _sqlGenerator;
        private readonly IDbCommand _cmd;

        public Query(ICommandManager commandManager, ISqlGenerator<T> sqlGenerator)
        {
            _commandManager = commandManager;
            _cmd = commandManager.GetNewCommand();
            _sqlGenerator = sqlGenerator;
        }
        public T Get(TKey id)
        {
            _commandManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetSelectByIdQuery(), new { id });
            return Mapper.GetObject<T>(_cmd);
        }

        public IEnumerable<T> GetAll()
        {
            _commandManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetSelectAllQuery());
            return Mapper.GetObjects<T>(_cmd);
        }

        public T Find(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[propertyName] = ReflectionUtilities.GetValueFromExpression(expression);
            var columnName = _sqlGenerator.NamingConvention.ConvertToDbName(propertyName);
            _commandManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetSelectByColumnNameQuery(columnName), obj);
            return Mapper.GetObject<T>(_cmd);

        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            var columnName = _sqlGenerator.NamingConvention.ConvertToDbName(propertyName);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[propertyName] = ReflectionUtilities.GetValueFromExpression(expression);
            _commandManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetSelectByColumnNameQuery(columnName), obj);
            return Mapper.GetObjects<T>(_cmd);
        }

        public PagedList<T> GetPagedList(int page, string orderBy = "id")
        {
            try
            {
                _commandManager.BeginTransaction(_cmd);
                var mainSql = _sqlGenerator.GetPagedQuery(page, orderBy);
                _commandManager.PrepareCommandForExecution(_cmd, mainSql);
                IList<T> list = Mapper.GetObjects<T>(_cmd).ToList();
                var countSql = _sqlGenerator.GetCountQuery();
                _commandManager.PrepareCommandForExecution(_cmd, countSql);
                var pagedList = new PagedList<T>(Mapper.ExecuteScalar<long>(_cmd), page)
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

    }
}