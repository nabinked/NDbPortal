using System;
using System.Data;
using NDbPortal.Names;

namespace NDbPortal
{
    public class CommandBuilder : ICommandBuilder
    {
        private readonly INamingConvention _namingConvention;
        private readonly ParamParser _paramParser;

        public CommandBuilder(INamingConvention namingConvention)
        {
            _namingConvention = namingConvention;
            _paramParser = new ParamParser(namingConvention);
        }

        #region Public Behaviours

        public IDbCommand GetFinalCommand(IDbCommand cmd, string sql, object parameters = null)
        {
            return GetCommand(cmd, sql, parameters);
        }

        #endregion

        #region Private Behaviours
        IDbCommand GetCommand(IDbCommand dbCommand, string sql, object parameters = null)
        {
            dbCommand.Parameters.Clear();
            dbCommand.CommandText = sql;
            if (parameters != null)
            {
                if (dbCommand.CommandType == CommandType.StoredProcedure)
                {
                    foreach (string propName in ReflectionUtilities.GetProperties(parameters))
                    {
                        dbCommand.Parameters.Add(GetParam(dbCommand, _namingConvention.ConvertToDbName(propName), ReflectionUtilities.GetPropertyValue(propName, parameters)));
                    }
                }
                else
                {
                    var sqlParams = _paramParser.GeSqlParams(sql);
                    foreach (var sqlParam in sqlParams)
                    {
                        var cleanParam = _paramParser.CleanParameter(sqlParam);
                        var paramValue = ReflectionUtilities.GetPropertyValue(_namingConvention.ConvertToPocoName(cleanParam), parameters) ?? DBNull.Value;
                        var parameter = GetParam(dbCommand, cleanParam, paramValue);
                        dbCommand.Parameters.Add(parameter);
                    }
                }

            }
            return dbCommand;
        }
        IDbDataParameter GetParam(IDbCommand dbCommand, string paramName, object paramValue)
        {
            var param = dbCommand.CreateParameter();
            param.ParameterName = _paramParser.AddParamIdentifier(paramName);
            param.Value = paramValue;
            return param;
        }
        #endregion
    }
}
