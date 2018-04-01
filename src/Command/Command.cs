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
            _sqlGenerator = sqlGenerator;
            _cmd = cmdManager.GetNewCommand();
        }

        public long Add(T obj)
        {
            _cmdManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetInsertQuery(), obj);
            return Mapper.GetObject<long>(_cmd);
        }

        public IList<long> AddRange(IEnumerable<T> entities)
        {
            var returnIds = new List<long>();
            try
            {
                BeginTransaction();
                foreach (var entity in entities)
                {
                    _cmdManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetInsertQuery(), entity);
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
            _cmdManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetUpdateQuery(), obj);
            return Mapper.ExecuteNonQuery(_cmd);
        }

        public long UpdateRange(IEnumerable<T> entities)
        {
            long affectedRecordsCount = 0;
            try
            {
                BeginTransaction();
                foreach (var entity in entities)
                {
                    _cmdManager.PrepareCommandForExecution(_cmd, _sqlGenerator.GetUpdateQuery(), entity);
                    affectedRecordsCount += Mapper.ExecuteNonQuery(_cmd);
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
            _cmdManager.PrepareCommandForExecution(_cmd,_sqlGenerator.GetDeleteQuery(), new { id });
            return Mapper.ExecuteNonQuery(_cmd) > 0;
        }

        public bool RemoveRange(List<TPrimary> idsList)
        {
            try
            {
                BeginTransaction();
                foreach (var id in idsList)
                {
                    _cmdManager.PrepareCommandForExecution(_cmd,_sqlGenerator.GetDeleteQuery(), new { id });
                    Mapper.ExecuteNonQuery(_cmd);
                }
                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            return true;
        }


        public long Upsert(T obj)
        {
            return IsNew(obj) ? Add(obj) : Update(obj);
        }

        public void BeginTransaction()
        {
            _cmdManager.BeginTransaction(_cmd);
        }

        public void CommitTransaction()
        {
            _cmdManager.CommitTransaction(_cmd);
        }

        public void RollbackTransaction()
        {
            _cmdManager.RollbackTransaction(_cmd);
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

            if (primaryKeyValue == null ||
                primaryKeyValue.Equals(Activator.CreateInstance(primaryKeyProperty.PropertyType)))
            {
                return true;
            }
            return false;
        }

        #endregion

    }
}