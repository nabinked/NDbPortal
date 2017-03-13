using System;

namespace NDbPortal
{
    public interface INamingConvention
    {
        DbEnums.NamingConventions DbNamingConvention { get; set; }

        DbEnums.NamingConventions PocoNamingConvention { get; set; }

        string ConvertToDbName(string pocoName);
        string ConvertToPocoName(string dbName);
    }
}
