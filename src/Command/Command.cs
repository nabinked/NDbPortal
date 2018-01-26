using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using NDbPortal.Names;

namespace NDbPortal.Command
{
    public class Command<T, TPrimary> : ICommand<T, TPrimary> where T : class
    {
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;
        private readonly ICommandBuilder _cmdBuilder;
        private readonly DbOptions _dbOptions;
        private readonly SqlGenerator _sqlGenerator;
        private readonly IDbCommand _cmd;
        private readonly short _pageSize;


        public Command(ICommandFactory commandFactory, INamingConvention namingConvention, IOptions<DbOptions> dbOptions, ICommandBuilder cmdBuilder)
        {
            _namingConvention = namingConvention;
            _cmdBuilder = cmdBuilder;
            _dbOptions = dbOptions.Value;
            _commandFactory = commandFactory;
            _sqlGenerator = new SqlGenerator(GetTableInfoBuilder().Build(), namingConvention);
            _pageSize = (short)(dbOptions.Value.PagedListSize > 0 ? dbOptions.Value.PagedListSize : 10);
            _cmd = commandFactory.Create();
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
            try
            {
                BeginTransaction();
                foreach (var entity in entities)
                {
                    var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetInsertQuery(), entity);
                    returnIds.Add(Mapper.GetObject<long>(_cmd, false));
                }
                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                returnIds.Clear();
            }
            return returnIds;
        }

        public long Update(T obj)
        {
            using (var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetUpdateQuery(), obj))
            {
                return Mapper.ExecuteNonQuery(finalCommand);
            }
        }

        public long UpdateRange(IEnumerable<T> entities)
        {
            long affectedRecordsCount = 0;
            try
            {
                BeginTransaction();
                foreach (var entity in entities)
                {
                    var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetUpdateQuery(), entity);
                    affectedRecordsCount += Mapper.ExecuteNonQuery(finalCommand, false);
                }
                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                affectedRecordsCount = 0;
            }
            return affectedRecordsCount;
        }

        public bool Remove(TPrimary id)
        {
            using (var finalCommand = _cmdBuilder.GetFinalCommand(_cmd, _sqlGenerator.GetDeleteQuery(), new { id }))
            {
                return Mapper.ExecuteNonQuery(finalCommand) > 0;
            }
        }

        public bool Remove(T obj)
        {
            var primaryKeyName = ReflectionUtilities.GetPrimaryKey(typeof(TPrimary));
            var id = ReflectionUtilities.GetPropertyValue<T,TPrimary>(primaryKeyName, obj);
            return Remove(id);
        }

        public List<int> RemoveRange(List<TPrimary> idsList)
        {
            
        }

        public List<int> RemoveRange(List<T> objs)
        {
            
        }

        public long Upsert(T obj)
        {
            return IsNew(obj) ? Add(obj) : Update(obj);
        }

        public void BeginTransaction()
        {
            if (_cmd.Connection.State != ConnectionState.Open)
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