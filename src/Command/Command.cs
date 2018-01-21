using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using NDbPortal.Names;

namespace NDbPortal.Command
{
    public class Command<T> : ICommand<T> where T : class
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;
        private readonly ICommandBuilder _cmdBuilder;
        private readonly DbOptions _dbOptions;
        private readonly SqlGenerator _sqlGenerator;
        private readonly short _pageSize;
        private IDbTransaction _transaction;

        public Command(IConnectionFactory connectionFactory, ICommandFactory commandFactory,
                            INamingConvention namingConvention, IOptions<DbOptions> dbOptions,
                            ICommandBuilder cmdBuilder)
        {
            _namingConvention = namingConvention;
            _cmdBuilder = cmdBuilder;
            _dbOptions = dbOptions.Value;
            _connectionFactory = connectionFactory;
            _commandFactory = commandFactory;
            _sqlGenerator = new SqlGenerator(GetTableInfoBuilder().Build(), namingConvention);
            _pageSize = (short)(dbOptions.Value.PagedListSize > 0 ? dbOptions.Value.PagedListSize : 10);
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

        public long UpdateRange(IEnumerable<T> entities)
        {
            long affectedRecordsCount = 0;
            using (var conn = _connectionFactory.Create())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                var cmd = conn.CreateCommand();
                foreach (var entity in entities)
                {
                    cmd = _cmdBuilder.GetFinalCommand(cmd, _sqlGenerator.GetUpdateQuery(), entity);
                    affectedRecordsCount += Mapper.GetObject<long>(cmd, false);
                }
                transaction.Commit();
            }
            return affectedRecordsCount;
        }

        public bool Remove(long id)
        {
            using (var cmd = _commandFactory.Create(_sqlGenerator.GetDeleteQuery(), new { id }))
            {
                return Mapper.ExecuteNonQuery(cmd) > 0;
            }
        }

        public long Upsert(T obj)
        {
            return IsNew(obj) ? Add(obj) : Update(obj);
        }

        public void BeginTransaction()
        {
            using (var conn = _connectionFactory.Create())
            {
                _transaction = conn.BeginTransaction();
            }
        }


        #region Privates
        /// <summary>
        /// Checks if the object primary key has any value to determine if its new.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool IsNew(T t)
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