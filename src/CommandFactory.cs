using System.Data;

namespace NDbPortal
{
    public class CommandFactory : ICommandFactory
    {
        private readonly IConnectionFactory _connectionFactory;

        public CommandFactory(IConnectionFactory connectionFactory)
        {
            this._connectionFactory = connectionFactory;
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
            var cmdTextParam = new CommandBuilder(cmd);
            return cmdTextParam.GetFinalCommand(commandText, parameters);
        }

        public IDbCommand Create(IDbCommand cmd, string commandText, object parameters = null)
        {
            var cmdTextParam = new CommandBuilder(cmd);
            return cmdTextParam.GetFinalCommand(commandText, parameters);

        }
    }
}
