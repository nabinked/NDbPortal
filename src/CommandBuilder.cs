using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NDbPortal.Names;

namespace NDbPortal
{
    public class CommandBuilder
    {
        private readonly IDbCommand _dbCommand;
        private readonly NamingConvention _namingConvention;

        public CommandBuilder(IDbCommand dbCommand, NamingConvention namingConvention = null)
        {
            _dbCommand = dbCommand;
            _dbCommand.Parameters.Clear();
            this._namingConvention = namingConvention ?? new NamingConvention()
            {
                DbNamingConvention = DbEnums.NamingConventions.UnderScoreCase,
                PocoNamingConvention = DbEnums.NamingConventions.PascalCase
            };
        }

        #region Public Behaviours

        public IDbCommand GetFinalCommand(string sql, object parameters = null)
        {
            return GetCommand(sql, parameters);
        }

        #endregion


        #region Private Behaviours
        IDbCommand GetCommand(string sql, object parameters = null)
        {
            _dbCommand.CommandText = sql;
            if (parameters != null)
            {
                if (_dbCommand.CommandType == CommandType.StoredProcedure)
                {
                    foreach (string propName in ReflectionUtilities.GetProperties(parameters))
                    {
                        _dbCommand.Parameters.Add(GetParam(_dbCommand, $"@{_namingConvention.ConvertToDbName(propName)}", ReflectionUtilities.GetPropertyValue(propName, parameters)));
                    }
                }
                else
                {
                    var sqlParams = GeSqlParams(sql);
                    foreach (var sqlParam in sqlParams)
                    {
                        var paramValue = ReflectionUtilities.GetPropertyValue(CleanParameter(sqlParam), parameters) ?? DBNull.Value;
                        var parameter = GetParam(_dbCommand, sqlParam, paramValue);
                        _dbCommand.Parameters.Add(parameter);
                    }
                }

            }
            return _dbCommand;
        }
        IDbDataParameter GetParam(IDbCommand dbCommand, string paramName, object paramValue)
        {
            var param = dbCommand.CreateParameter();
            param.ParameterName = paramName;
            param.Value = paramValue;
            //param.DbType = TypeToDbTypeMap.GetDbType(paramValue.GetType());
            return param;
        }

        IEnumerable<string> GeSqlParams(string sql)
        {
            var sqlParams = new List<string>();
            var paramStartIndex = 0;
            var paramFound = false;
            for (var i = 0; i < sql.Length; i++)
            {
                var c = sql[i];

                switch (c)
                {
                    case '?':
                    case '@':
                        paramFound = true;
                        paramStartIndex = i;
                        break;
                }
                var endOfSql = i == sql.Length - 1;
                if ((char.IsWhiteSpace(c) || c.Equals(',') || endOfSql) && paramFound)
                {
                    paramFound = false;
                    var paramStopIndex = endOfSql ? i + 1 : i;
                    var param = RemoveAll(sql.Substring(paramStartIndex, paramStopIndex - paramStartIndex), (new[] { ";", ")", "," }));
                    if (!sqlParams.Contains(param))
                    {
                        sqlParams.Add(param);
                    }
                }
            }
            return sqlParams;
        }

        static string RemoveAll(string str, string[] stringToReplace)
        {
            var sb = new StringBuilder(str);

            foreach (var s in stringToReplace)
            {
                if (string.IsNullOrEmpty(s)) continue;
                sb.Replace(s, "");
            }

            return sb.ToString();
        }

        static string CleanParameter(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                switch (name[0])
                {
                    case '@':
                    case '?':
                        return name.Substring(1).Trim();
                }
            }
            return name;

        }

        #endregion
    }
}
