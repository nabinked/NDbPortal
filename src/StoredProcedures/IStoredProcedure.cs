using System.Collections.Generic;
using NDbPortal.Names;

// ReSharper disable once CheckNamespace
namespace NDbPortal
{
    public interface IStoredProcedure
    {
        void Invoke<TParams>(string name, TParams prms = null) 
            where TParams : class;

        T Get<T, TParams>(TParams prm = null)
            where T : class
            where TParams : class;

        T Get<T, TParams>(string name, TParams prm = null)
            where T : class
            where TParams : class;

        IEnumerable<T> GetList<T, TParams>(TParams prm = null)
            where T : class
            where TParams : class;

        IEnumerable<T> GetList<T, TParams>(string name, TParams prm = null) 
            where T : class
            where TParams : class;


        PagedList<T> GetPagedList<T, TParams>(long page, TParams prm = null)
            where T : class
            where TParams : class;

        PagedList<T> GetPagedList<T, TParams>(string name, long page, TParams prm = null)
            where T : class
            where TParams : class;

    }
}
