using System;
using System.Collections.Generic;
using Npgsql;

namespace NDbPortal
{
    public class DbOptions
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public DbEnums.NamingConventions DbNamingConvention { get; set; }
        public DbEnums.NamingConventions ModelNamingConvention { get; set; }
        public IEnumerable<Type> Enums { get; set; }
        public short PagedListSize { get; set; }
        public DbEnums.DbType DbType { get; set; }

        public string GetConnectionString()
        {
            switch (DbType)
            {
                case (DbEnums.DbType.Postgres):
                {
                    if (!string.IsNullOrWhiteSpace(ConnectionStrings.NpgsqlConnectionStringOptions.Database))
                        return new NpgsqlConnectionStringBuilder()
                        {
                            Host = ConnectionStrings.NpgsqlConnectionStringOptions.Server,
                            Password = ConnectionStrings.NpgsqlConnectionStringOptions.Password,
                            Database = ConnectionStrings.NpgsqlConnectionStringOptions.Database,
                            Username = ConnectionStrings.NpgsqlConnectionStringOptions.UserId,
                            Port = ConnectionStrings.NpgsqlConnectionStringOptions.Port
                        }.ConnectionString;
                    else return ConnectionStrings.DefaultConnectionString;
                }
                case (DbEnums.DbType.MsSql):
                case (DbEnums.DbType.MySql):
                    {
                        return ConnectionStrings.DefaultConnectionString;
                    }
                default:
                    return ConnectionStrings.DefaultConnectionString;

            }
        }
    }
}
