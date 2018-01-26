using System.Data;

namespace NDbPortal
{
    public interface ICommandManager
    {
        IDbCommand GetCommand();
        IDbCommand PrepareCommandForExecution(string sql, object parameters = null);
    }
}