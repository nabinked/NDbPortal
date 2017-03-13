using System.Collections.Generic;
using System.Linq;

namespace NDbPortal
{
    public class SqlGenerator
    {
        private readonly TableInfo _tableInfo;

        private string WhereId => $" WHERE {_tableInfo.PrimaryKey} = @{_tableInfo.PrimaryKey}";

        public SqlGenerator(TableInfo tableInfo)
        {
            _tableInfo = tableInfo;
        }

        #region Public Members
        /// <summary>
        /// Generest SELECT * query
        /// </summary>
        /// <returns></returns>
        public string GetSelectAllQuery()
        {
            return $"SELECT * FROM {_tableInfo.FullTableName};";
        }

        /// <summary>
        /// Generest a sql query that SELECTs by id. SQL Parameter id
        /// </summary>
        /// <returns></returns>
        public string GetSelectByIdQuery()
        {
            return $"SELECT * FROM {_tableInfo.FullTableName} {WhereId};";
        }

        public string GetSelectByColumnNameQuery(string columnName)
        {
            return $"SELECT * FROM {_tableInfo.FullTableName} WHERE LOWER({columnName} ::text) = LOWER(@{columnName} ::text);";
        }

        /// <summary>
        /// Creates an insert query. Parameters column values
        /// </summary>
        /// <returns></returns>
        public string GetInsertQuery()
        {
            var columnNameList = _tableInfo.InsertUpdateColumns.Select(x => x.ColumnName);
            var parameterNames = string.Join(", ", columnNameList.Select(insertColumName => $"@{insertColumName.Replace("_", "")}").ToList());
            var sql =
                $"INSERT INTO {_tableInfo.FullTableName} ({_tableInfo.InsertUpdateColumnsString}) VALUES ({parameterNames})";
            return sql + $" RETURNING {_tableInfo.PrimaryKey};";
        }

        public string GetDeleteQuery()
        {
            return $"DELETE FROM {_tableInfo.FullTableName} {WhereId};";
        }

        /// <summary>
        /// Creates a Update query
        /// </summary>
        /// <returns>sql query</returns>
        public string GetUpdateQuery()
        {
            var columnNameList = _tableInfo.Columns.Select(x => x.ColumnName).ToList();
            var setString = GetSetStringForUpdateQuery(columnNameList);
            var sql = $"UPDATE {_tableInfo.FullTableName} SET {setString} {WhereId};";
            return sql;

            ;
        }

        public string GetPagedQuery(string orderByColumnName)
        {
            return $"SELECT * FROM {_tableInfo.FullTableName} ORDER BY {orderByColumnName} DESC LIMIT @limit OFFSET @offset";
        }

        public string GetCountQuery()
        {
            return $"SELECT COUNT(*) FROM {_tableInfo.FullTableName};";
        }
        #endregion


        #region Private Members
        private string GetSetStringForUpdateQuery(IList<string> updatableColumnNames)
        {
            var setString = "";
            for (int i = 0; i < updatableColumnNames.Count; i++)
            {
                var columnName = updatableColumnNames[i];
                setString += columnName + " = @" + columnName.Replace("_", "");
                if (i != updatableColumnNames.Count - 1)
                {
                    setString += " ,";
                }
            }
            return setString;
        }
        #endregion

    }

}

