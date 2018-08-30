using System;
using System.Data;
using NDbPortal.Names;

namespace NDbPortal
{
    public class CommandManager : ICommandManager
    {
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;
        private readonly IParamParser _paramParser;

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

        public IDbCommand PrepareCommandForExecution(string sql, object parameters = null, IDbCommand cmd = null, CommandType commandType = CommandType.Text)
        {
            commandType = commandType < CommandType.Text ? CommandType.Text : commandType;
            cmd = cmd ?? GetNewCommand(commandType);
            PopulateParameters(cmd, sql, parameters);
            return cmd;
        }

        public IDbCommand BeginTransaction()
        {
            var cmd = GetNewCommand();
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            cmd.Connection.BeginTransaction();
            return cmd;
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
            if (parameters == null) return;
            if (cmd.CommandType == CommandType.StoredProcedure)
            {
                foreach (var propName in ReflectionUtilities.GetProperties(parameters))
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
