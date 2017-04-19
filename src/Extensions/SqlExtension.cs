namespace NDbPortal.Extensions
{
    public static class SqlExtension
    {
        public static string AppendLimitOffset(this string sql, int pageSize, long skip)
        {
            sql += $" LIMIT {pageSize} OFFSET {skip}";
            return sql;
        }
    }
}
