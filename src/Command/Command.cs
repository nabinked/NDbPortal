using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NDbPortal.Command
{
    public class Command<T> : ICommand<T> where T : class
    {
        private readonly ICommandManager _cmdManager;
        private readonly ISqlGenerator<T> _sqlGenerator;

        public Command(ICommandManager cmdManager, ISqlGenerator<T> sqlGenerator)
        {
            _cmdManager = cmdManager;
            _sqlGenerator = sqlGenerator;
        }

        public long Add(T obj)
        {
            var cmd = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetInsertQuery(), obj);
            var ret = Mapper.GetObject<long>(cmd);
            Dispose(cmd);
            return ret;
        }

        public IList<long> AddRange(IEnumerable<T> entities)
        {
            var returnIds = new List<long>();
            var cmd = BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetInsertQuery(), entity, cmd);
                    returnIds.Add(Mapper.GetObject<long>(cmd));
                }
                CommitTransaction(cmd);
            }
            catch
            {
                returnIds.Clear();
                RollbackTransaction(cmd);
                throw;
            }
            return returnIds;
        }

        public long Update(T obj)
        {
            var cmd = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetUpdateQuery(), obj);
            var ret = Mapper.ExecuteNonQuery(cmd);
            Dispose(cmd);
            return ret;
        }

        public long UpdateRange(IEnumerable<T> entities)
        {
            long affectedRecordsCount = 0;
            var cmd = BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetUpdateQuery(), entity, cmd);
                    affectedRecordsCount += Mapper.ExecuteNonQuery(cmd);
                }
                CommitTransaction(cmd);
            }
            catch
            {
                RollbackTransaction(cmd);
                affectedRecordsCount = 0;
            }
            return affectedRecordsCount;
        }

        public bool Remove(object id)
        {
            var cmd = _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetDeleteQuery(), new { id });
            var ret= Mapper.ExecuteNonQuery(cmd) > 0;
            Dispose(cmd);
            return ret;
        }

        public bool RemoveRange<TKey>(IEnumerable<TKey> idsList)
        {
            var cmd = BeginTransaction();

            try
            {
                foreach (var id in idsList)
                {
                    _cmdManager.PrepareCommandForExecution(_sqlGenerator.GetDeleteQuery(), new { id }, cmd);
                    Mapper.ExecuteNonQuery(cmd);
                }
                CommitTransaction(cmd);
            }
            catch
            {
                RollbackTransaction(cmd);
                throw;
            }
            return true;
        }


        public long Upsert(T t)
        {
            return IsNew(t) ? Add(t) : Update(t);
        }

        public IDbCommand BeginTransaction()
        {
            return _cmdManager.BeginTransaction();
        }

        public void CommitTransaction(IDbCommand cmd)
        {
            _cmdManager.CommitTransaction(cmd);
        }

        public void RollbackTransaction(IDbCommand cmd)
        {
            _cmdManager.RollbackTransaction(cmd);
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
            var primaryKeyProperty =
                t.GetType()
                    .GetProperties()
                    .FirstOrDefault(x => x.Name.Equals(primaryKey, StringComparison.OrdinalIgnoreCase));
            var primaryKeyValue = primaryKeyProperty.GetGetMethod().Invoke(t, null);

            return primaryKeyValue == null ||
                   primaryKeyValue.Equals(Activator.CreateInstance(primaryKeyProperty.PropertyType));
        }


        private void Dispose(IDbCommand cmd)
        {
            cmd.Transaction?.Commit();
            cmd.Connection?.Close();
            cmd.Connection?.Dispose();
            cmd?.Dispose();
        }

        #endregion

    }
}