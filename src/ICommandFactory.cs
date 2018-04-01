using System.Data;

namespace NDbPortal
{
    public interface ICommandFactory
    {
        IDbCommand Create();
        IDbCommand Create(bool isStoredProcedure);
        IDbCommand Create(IDbConnection connection, bool isStoredProcedure = false);
        void AttachConnection(IDbCommand cmd);
    }
}
