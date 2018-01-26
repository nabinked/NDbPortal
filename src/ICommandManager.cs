using System.Data;

namespace NDbPortal
{
    public interface ICommandManager
    {
        IDbCommand GetNewCommand();
        void PrepareCommandForExecution(IDbCommand cmd, string sql, object parameters = null);
        void BeginTransaction(IDbCommand cmd);
        void CommitTransaction(IDbCommand cmd);
        void RollbackTransaction(IDbCommand cmd);
    }
}