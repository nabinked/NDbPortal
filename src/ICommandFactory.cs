using System.Data;

namespace NDbPortal
{
    public interface ICommandFactory
    {
        IDbCommand Create(string sqlStatement = null, object parameters = null, bool isStoredProcedure = false);
        IDbCommand Create(IDbConnection connection, string sqlStatement = null, object parameters = null, bool isStoredProcedure = false);
    }
}
