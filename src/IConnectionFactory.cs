using System.Data;

namespace NDbPortal
{
    public interface IConnectionFactory
    {
        IDbConnection Create();

    }
}
