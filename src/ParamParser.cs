using System.Collections.Generic;
using System.Linq;
using NDbPortal.Names;

namespace NDbPortal
{
    public class ParamParser : IParamParser
    {
        private readonly INamingConvention _namingConvention;
        private readonly char[] _paramIdentifiers;
        private readonly char _defaultIdentifier;
        public ParamParser(INamingConvention namingConvention)
        {
            _namingConvention = namingConvention;
            _paramIdentifiers = new[] { '@', '?' };
            _defaultIdentifier = _paramIdentifiers[0];

        }

        public string GetStoredProcParamsNameValuesOnlySql(object obj)
        {
            var paramsSql = string.Empty;
            if (obj != null)
            {
                var properties = obj.GetType().GetProperties().OrderBy(x => x.Name);
                var paramNameValues = new List<string>();
                foreach (var info in properties)
                {
                    paramNameValues.Add($"{_namingConvention.ConvertToDbName(info.Name)} => {AddParamIdentifier(_namingConvention.ConvertToDbName(info.Name))}");
                }
                paramsSql = string.Join(",", paramNameValues);
            }

            return paramsSql;
        }

        public string AddParamIdentifier(string paramName)
        {
            return _defaultIdentifier + paramName;
        }

        public string CleanParameter(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (_paramIdentifiers.Contains(name[0]))
                    return name.Substring(1).Trim();
            }
            return name;

        }

        public IEnumerable<string> GeSqlParams(string sql)
        {
            var sqlParams = new List<string>();
            var paramStartIndex = 0;
            var paramFound = false;
            for (var i = 0; i < sql.Length; i++)
            {
                var c = sql[i];

                if (_paramIdentifiers.Contains(c))
                {
                    paramFound = true;
                    paramStartIndex = i;
                }
                var endOfSql = i == sql.Length - 1;
                if ((char.IsWhiteSpace(c) || c.Equals(',') || endOfSql) && paramFound)
                {
                    paramFound = false;
                    var paramStopIndex = endOfSql ? i + 1 : i;
                    var param = Utils.RemoveAll(sql.Substring(paramStartIndex, paramStopIndex - paramStartIndex), (new[] { ";", ")", "," }));
                    if (!sqlParams.Contains(param))
                    {
                        sqlParams.Add(param);
                    }
                }
            }
            return sqlParams;
        }

        public string GetSetStringForUpdateQuery(IList<string> updatableColumnNames)
        {
            var setString = "";
            for (int i = 0; i < updatableColumnNames.Count; i++)
            {
                var columnName = updatableColumnNames[i];
                setString += $"{columnName} = {AddParamIdentifier(columnName)}";
                if (i != updatableColumnNames.Count - 1)
                {
                    setString += " ,";
                }
            }
            return setString;
        }

        public string ConvertToParamNames(IEnumerable<string> columnNameList)
        {
            return string.Join(", ", columnNameList.Select(insertColumName => $"{AddParamIdentifier(insertColumName)}").ToList());
        }
    }
}
