using System.Collections.Generic;

namespace NDbPortal
{
    public interface IParamParser
    {
        string AddParamIdentifier(string paramName);
        string CleanParameter(string name);
        string ConvertToParamNames(IEnumerable<string> columnNameList);
        IEnumerable<string> GeSqlParams(string sql);
        string GetSetStringForUpdateQuery(IList<string> updatableColumnNames);
        string GetStoredProcParamsNameValuesOnlySql(object obj);
    }
}