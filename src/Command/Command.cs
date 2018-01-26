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
        private readonly ICommandManager _cmdManager;
        private readonly ISqlGenerator<T> _sqlGenerator;
        private readonly IDbCommand _cmd;


        public Command(ICommandManager cmdManager, ISqlGenerator<T> sqlGenerator)
        {
            _cmdManager = cmdManager;
            _cmd = cmdManager.GetCommand();
            _sqlGenerator = sqlGenerator;
        }

        public long Add(T obj)
        {
            using (var finalCmd = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetInsertQuery(), obj))
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
                    var finalCommand = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetInsertQuery(), entity);
                    returnIds.Add(Mapper.GetObject<long>(_cmd));
                }
                CommitTransaction();
            }
            catch
            {
                returnIds.Clear();
                RollbackTransaction();
                throw;
            }
            return returnIds;
        }

        public long Update(T obj)
        {
            using (var finalCommand = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetUpdateQuery(), obj))
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
                    var finalCommand = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetUpdateQuery(), entity);
                    affectedRecordsCount += Mapper.ExecuteNonQuery(finalCommand);
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
            using (var finalCommand = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetDeleteQuery(), new { id }))
            {
                return Mapper.ExecuteNonQuery(finalCommand) > 0;
            }
        }

        public bool Remove(T obj)
        {
            var primaryKeyName = _sqlGenerator.TableInfo.PrimaryKey;
            var id = ReflectionUtilities.GetPropertyValue<TPrimary>(primaryKeyName, obj);
            return Remove(id);
        }

        public bool RemoveRange(List<TPrimary> idsList)
        {
            foreach (TPrimary id in idsList)
            {

            }

            return false;
        }

        public bool RemoveRange(List<T> objs)
        {
            return false;
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
            var primaryKey = _sqlGenerator.TableInfo.PrimaryKey;
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

        #endregion

    }
}