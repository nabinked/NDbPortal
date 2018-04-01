namespace NDbPortal.StoredProcedures
{
    public interface IStoredProcedureSql
    {
        string GetStoredProcCountQuery(string storedProcedureName, object prms = null);
        string GetStoredProcQuery(string storedProcedureName, object prms = null);
    }
}