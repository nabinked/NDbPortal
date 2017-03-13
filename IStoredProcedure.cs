using System.Collections.Generic;

namespace NDbPortal
{
    public interface IStoredProcedure
    {
        void Invoke(string name, object prms = null);
        T Get<T>(object prm = null);
        IEnumerable<T> GetList<T>(object prm = null);
        T Get<T>(string name, object prm = null);

        IEnumerable<T> GetPaged<T>(string pageProcName, int pageSize, int skip, string searchText,
            object prm = null);
        IEnumerable<T> GetList<T>(string name, object prm = null);

    }
}
