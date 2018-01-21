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
        private readonly IDbCommand _cmd;
        private readonly short _pageSize;


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
            this._cmd = commandFactory.Create(connectionFactory.Create());
        }

        public long Add(T obj)
        {
            using (var finalCmd = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetInsertQuery(), obj))
            {
                return Mapper.GetObject<long>(finalCmd);
            }
        }

        public IList<long> AddRange(IEnumerable<T> entities)
        {
            var returnIds = new List<long>();
            BeginTransaction();
            foreach (var entity in entities)
            {
                var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetInsertQuery(), entity);
                returnIds.Add(Mapper.GetObject<long>(_cmd, false));
            }
            CommitTransaction();
            return returnIds;
        }

        public long Update(T obj)
        {
            using (var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetUpdateQuery(), obj))
            {
                return Mapper.GetObject<long>(finalCommand);
            }
        }

        public long UpdateRange(IEnumerable<T> entities)
        {
            long affectedRecordsCount = 0;
            BeginTransaction();
            foreach (var entity in entities)
            {
                var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetUpdateQuery(), entity);
                affectedRecordsCount += Mapper.GetObject<long>(finalCommand, false);
            }
            return affectedRecordsCount;
        }

        public bool Remove(long id)
        {
            using (var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetDeleteQuery(), new { id }))
            {
                return Mapper.ExecuteNonQuery(finalCommand) > 0;
            }
        }

        public long Upsert(T obj)
        {
            return IsNew(obj) ? Add(obj) : Update(obj);
        }

        public void BeginTransaction()
        {
            if (_cmd.Connection.State == ConnectionState.Open)
            {
                _cmd.Connection.Open();
            }

            _cmd.Transaction = _cmd.Connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _cmd.Transaction.Commit();
        }

        public void RollbackTransaction()
        {
            _cmd.Transaction.Rollback();
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