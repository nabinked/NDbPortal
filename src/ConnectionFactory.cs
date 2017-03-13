using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace NDbPortal
{
    public class ConnectionFactory : IConnectionFactory
    {
        private string ConnectionString { get; set; }

        public IDbConnection Create()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            return connection;
        }

        public ConnectionFactory(IOptions<DbOptions> dbOptions)
        {
            ConnectionString = dbOptions.Value.GetConnectionString();
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
    }
}
