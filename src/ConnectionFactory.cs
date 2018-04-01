using System.Data;
using Microsoft.Extensions.Options;
using NDbPortal.Names;
using Npgsql;

namespace NDbPortal
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly DbOptions _dbOptions;
        private string ConnectionString { get; set; }

        public IDbConnection Create()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            return connection;
        }

        public ConnectionFactory(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions.Value;
            ConnectionString = GetConnectionString();
        }

        public ConnectionFactory(string connectionString)
        {
            ConnectionString = connectionString;
        }
        
        public IDbCommand CreateCommand()
        {
            return Create().CreateCommand();
        }

        public string GetConnectionString()
        {
            switch (_dbOptions.DbType)
            {
                case (DbEnums.DbType.Postgres):
                    {
                        if (!string.IsNullOrWhiteSpace(_dbOptions.ConnectionStrings.NpgsqlConnectionStringOptions.Database))
                            return new NpgsqlConnectionStringBuilder()
                            {
                                Host = _dbOptions.ConnectionStrings.NpgsqlConnectionStringOptions.Server,
                                Password = _dbOptions.ConnectionStrings.NpgsqlConnectionStringOptions.Password,
                                Database = _dbOptions.ConnectionStrings.NpgsqlConnectionStringOptions.Database,
                                Username = _dbOptions.ConnectionStrings.NpgsqlConnectionStringOptions.UserId,
                                Port = _dbOptions.ConnectionStrings.NpgsqlConnectionStringOptions.Port
                            }.ConnectionString;
                        else return _dbOptions.ConnectionStrings.DefaultConnectionString;
                    }
                case (DbEnums.DbType.MsSql):
                case (DbEnums.DbType.MySql):
                    {
                        return _dbOptions.ConnectionStrings.DefaultConnectionString;
                    }
                default:
                    return _dbOptions.ConnectionStrings.DefaultConnectionString;

            }
        }
    }
}
