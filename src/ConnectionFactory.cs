using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using NDbPortal.Names;
using Npgsql;

namespace NDbPortal
{
    public class ConnectionFactory : IConnectionFactory
    {
        private DbOptions _dbOptions;
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
            //if (dbOptions.Value.Enums.Any())
            //{
            //    //_mapEnums = ;
            //    foreach (Type enumType in dbOptions.Value.Enums)
            //    {
            //        var methodInfo = typeof(NpgsqlConnection).GetMethod("MapEnumGlobally");
            //        var genMethod = methodInfo.MakeGenericMethod(enumType);
            //        genMethod.Invoke(null, new object[] { null, null });
            //    }
            //}


        }

        public ConnectionFactory(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IDbCommand CreateCommand(string sqlStatement, object parameters = null)
        {
            var cmd = CreateCommand();
            cmd.CommandText = sqlStatement;
            var cmdTextParam = new CommandBuilder(cmd);
            return cmdTextParam.GetFinalCommand(sqlStatement, parameters);
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
