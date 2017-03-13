using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace NDbPortal
{
    public class ConnectionStrings
    {
        public string DefaultConnectionString { get; set; }
        public NpgsqlConnectionStringOptions NpgsqlConnectionStringOptions { get; set; }
    }
}
