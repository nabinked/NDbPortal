using System.Data;

namespace NDbPortal
{
    public interface ICommandManager
    {
        IDbCommand GetNewCommand(CommandType commandType = CommandType.Text);
        IDbCommand PrepareCommandForExecution(string sql, object parameters = null, IDbCommand cmd = null, CommandType commandType = CommandType.Text);
        IDbCommand BeginTransaction();
        void CommitTransaction(IDbCommand cmd);
        void RollbackTransaction(IDbCommand cmd);
    }
}