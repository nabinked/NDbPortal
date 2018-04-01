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

        public CommandManager(ICommandFactory commandFactory, INamingConvention namingConvention)
        {
            _commandFactory = commandFactory;
            _namingConvention = namingConvention;
            _paramParser = new ParamParser(namingConvention);
        }

        #region Public Behaviours

        public IDbCommand GetNewCommand(CommandType commandType = CommandType.Text)
        {
            var cmd = _commandFactory.Create();
            cmd.CommandType = commandType;
            return cmd;
        }

        public void PrepareCommandForExecution(IDbCommand cmd, string sql, object parameters = null)
        {
            PopulateParameters(cmd, sql, parameters);
        }

        public void BeginTransaction(IDbCommand cmd)
        {
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }

            cmd.Transaction = cmd.Connection.BeginTransaction();
        }

        public void CommitTransaction(IDbCommand cmd)
        {
            cmd.Transaction?.Commit();
            cmd.Connection?.Close();
            cmd.Connection?.Dispose();
            cmd?.Dispose();
        }

        public void RollbackTransaction(IDbCommand cmd)
        {
            cmd.Transaction?.Rollback();
            cmd.Connection?.Close();
            cmd.Connection?.Dispose();
            cmd?.Dispose();
        }
        #endregion

        #region Private Behaviours
        void PopulateParameters(IDbCommand cmd, string sql, object parameters = null)
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
