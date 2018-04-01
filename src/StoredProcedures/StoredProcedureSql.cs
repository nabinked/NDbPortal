namespace NDbPortal.StoredProcedures
{
    public class StoredProcedureSql : IStoredProcedureSql
    {
        private readonly IParamParser _paramParser;

        public StoredProcedureSql(IParamParser paramParser)
        {
            _paramParser = paramParser;
        }

        public string GetStoredProcQuery(string storedProcedureName, object prms = null)
        {
            return $"SELECT * FROM {storedProcedureName}({_paramParser.GetStoredProcParamsNameValuesOnlySql(prms)})";
        }

        public string GetStoredProcCountQuery(string storedProcedureName, object prms = null)
        {
            return $"SELECT COUNT(*) FROM {storedProcedureName}({_paramParser.GetStoredProcParamsNameValuesOnlySql(prms)});";
        }
    }
}
