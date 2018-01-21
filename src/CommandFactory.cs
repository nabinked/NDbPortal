using System.Data;

namespace NDbPortal
{
    public class CommandFactory : ICommandFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ICommandBuilder _cmdBuilder;

        public CommandFactory(IConnectionFactory connectionFactory, ICommandBuilder cmdBuilder)
        {
            this._connectionFactory = connectionFactory;
            _cmdBuilder = cmdBuilder;
        }

        public CommandFactory(string connectionString)
        {
            this._connectionFactory = new ConnectionFactory(connectionString);
        }

        public IDbCommand Create(string commandText, object parameters = null, bool isStoredProcedure = false)
        {
            var cmd = _connectionFactory.Create().CreateCommand();
            if (isStoredProcedure)
            {
                cmd.CommandType = CommandType.StoredProcedure;
            }
            return _cmdBuilder.GetFinalCommand(cmd, commandText, parameters);
        }

        public IDbCommand Create(IDbConnection connection, string sqlStatement = null, object parameters = null,
            bool isStoredProcedure = false)
        {
            throw new System.NotImplementedException();
        }

    }
}
