using System.Collections.Generic;
using NDbPortal.Names;

namespace NDbPortal
{
    public interface IStoredProcedure
    {
        void Invoke(string name, object prms = null);

        T Get<T>(object prm = null);
        T Get<T>(string name, object prm = null);

        IEnumerable<T> GetList<T>(object prm = null);
        IEnumerable<T> GetList<T>(string name, object prm = null);

        PagedList<T> GetPagedList<T>(long page, object prm = null) where T : class;
        PagedList<T> GetPagedList<T>(string name, long page, object prm = null) where T : class;

    }
}
