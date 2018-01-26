using NDbPortal.Names;

namespace NDbPortal
{
    public interface ISqlGenerator<T>
    {
        INamingConvention NamingConvention { get; }
        TableInfo TableInfo { get; }
        string GetCountQuery();
        string GetDeleteQuery();
        string GetInsertQuery();
        string GetPagedQuery(int page, string orderByColumnName);
        string GetSelectAllQuery();
        string GetSelectByColumnNameQuery(string columnName);
        string GetSelectByIdQuery();
        string GetStoredProcCountQuery(object prms = null);
        string GetStoredProcQuery(object prms = null);
        string GetUpdateQuery();
    }
}