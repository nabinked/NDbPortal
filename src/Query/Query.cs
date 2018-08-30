using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using NDbPortal.Names;

namespace NDbPortal.Query
{
    public class Query<T> : IQuery<T> where T : class
    {
        private readonly ICommandManager _commandManager;
        private readonly ISqlGenerator<T> _sqlGenerator;

        public Query(ICommandManager commandManager, ISqlGenerator<T> sqlGenerator)
        {
            _commandManager = commandManager;
            _sqlGenerator = sqlGenerator;
        }

        public T Get(object id)
        {
            var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectByIdQuery(), new { id });
            var ret = Mapper.GetObject<T>(cmd);
            Dispose(cmd);
            return ret;
        }

        public IEnumerable<T> GetAll()
        {
            var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectAllQuery());
            var ret = Mapper.GetObjects<T>(cmd);
            Dispose(cmd);
            return ret;
        }

        public T Find(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[propertyName] = ReflectionUtilities.GetValueFromExpression(expression);
            var columnName = _sqlGenerator.NamingConvention.ConvertToDbName(propertyName);
            var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectByColumnNameQuery(columnName), obj);
            var ret = Mapper.GetObject<T>(cmd);
            Dispose(cmd);
            return ret;
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression)
        {
            var propertyName = ReflectionUtilities.GetPropertyNameFromExpression(expression);
            var columnName = _sqlGenerator.NamingConvention.ConvertToDbName(propertyName);
            dynamic obj = new ExpandoObject();
            var paramObj = (IDictionary<string, object>)obj;
            paramObj[propertyName] = ReflectionUtilities.GetValueFromExpression(expression);
            var cmd = _commandManager.PrepareCommandForExecution(_sqlGenerator.GetSelectByColumnNameQuery(columnName), obj);
            var ret = Mapper.GetObjects<T>(cmd);
            Dispose(cmd);
            return ret;
        }

        public IPagedList<T> GetPagedList(int page, string orderByColumn = "id")
        {
            var cmd = _commandManager.BeginTransaction();

            try
            {
                var mainSql = _sqlGenerator.GetPagedQuery(page, orderByColumn);
                _commandManager.PrepareCommandForExecution(mainSql, null, cmd);
                IList<T> list = Mapper.GetObjects<T>(cmd).ToList();
                var countSql = _sqlGenerator.GetCountQuery();
                _commandManager.PrepareCommandForExecution(countSql, cmd);
                var pagedList = new PagedList<T>(list, Mapper.ExecuteScalar<long>(cmd), page);
                _commandManager.CommitTransaction(cmd);
                return pagedList;
            }
            catch
            {
                _commandManager.RollbackTransaction(cmd);
                throw;
            }
        }

        private void Dispose(IDbCommand cmd)
        {
            cmd.Transaction?.Commit();
            cmd.Connection?.Close();
            cmd.Connection?.Dispose();
            cmd?.Dispose();
        }

    }

    public class Query : IQuery
    {
        private readonly ICommandManager _commandManager;

        public Query(ICommandManager commandManager)
        {
            _commandManager = commandManager;
        }
        public IEnumerable<T> ExecuteQuery<T>(string sql, object parameters)
        {
            var cmd = _commandManager.PrepareCommandForExecution(sql, parameters);
            var ret = Mapper.GetObjects<T>(cmd);
            Dispose(cmd);
            return ret;
        }
        private void Dispose(IDbCommand cmd)
        {
            Utils.Dispose(cmd);
        }
    }
}