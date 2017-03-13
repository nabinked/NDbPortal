using System.Collections.Generic;
using System.Linq;

namespace NDbPortal
{
    public class TableInfo
    {
        public IList<ColumnInfo> Columns { get; set; }
        public IEnumerable<ColumnInfo> InsertUpdateColumns { get; set; }
        public string InsertUpdateColumnsString => string.Join(", ", InsertUpdateColumns.Select(x => x.ColumnName));
        public string TableName { get; set; }
        public string FullTableName { get; set; }
        public string TableSchema { get; set; }
        public string PrimaryKey { get; set; }
    }
}
