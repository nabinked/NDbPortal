using System;
using System.Data;
using NDbPortal.Names;

namespace NDbPortal
{
    public class CommandManager : ICommandManager
    {
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;
        private readonly ParamParser _paramParser;
        private readonly IDbCommand _cmd;

        public CommandManager(ICommandFactory commandFactory, INamingConvention namingConvention)
        {
            _commandFactory = commandFactory;
            _namingConvention = namingConvention;
            _paramParser = new ParamParser(namingConvention);
            _cmd = commandFactory.Create();
        }

        #region Public Behaviours

        public IDbCommand GetCommand()
        {
            if (_cmd.Connection == null)
            {
                return _commandFactory.Create();
            }
            else
            {
                return _cmd;
            }
        }

        public IDbCommand PrepareCommandForExecution(string sql, object parameters = null)
        {
            return PopulateParameters(GetCommand(), sql, parameters);
        }

        #endregion

        #region Private Behaviours
        IDbCommand PopulateParameters(IDbCommand cmd, string sql, object parameters = null)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = sql;
            if (parameters != null)
            {
                if (cmd.CommandType == CommandType.StoredProcedure)
                {
                    foreach (string propName in ReflectionUtilities.GetProperties(parameters))
                    {
                        cmd.Parameters.Add(GetParam(cmd, _namingConvention.ConvertToDbName(propName), ReflectionUtilities.GetPropertyValue<object>(propName, parameters)));
                    }
                }
                else
                {
                    var sqlParams = _paramParser.GeSqlParams(sql);
                    foreach (var sqlParam in sqlParams)
                    {
                        var cleanParam = _paramParser.CleanParameter(sqlParam);
                        var pocoNme = _namingConvention.ConvertToPocoName(cleanParam);
                        var paramValue = ReflectionUtilities.GetPropertyValue<object>(pocoNme, parameters) ?? DBNull.Value;
                        var parameter = GetParam(cmd, cleanParam, paramValue);
                        cmd.Parameters.Add(parameter);
                    }
                }

            }
            return cmd;
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
