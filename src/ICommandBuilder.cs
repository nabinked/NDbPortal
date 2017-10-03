using System.Data;

namespace NDbPortal
{
    public interface ICommandBuilder
    {
        IDbCommand GetFinalCommand(IDbCommand cmd, string sql, object parameters = null);
    }
}