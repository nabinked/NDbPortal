using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NDbPortal.Names;

namespace NDbPortal
{
    public class SqlGenerator
    {
        private TableInfo _tableInfo;
        private INamingConvention _namingConvention;
        private ParamParser _paramParser;

        private string WhereId => $" WHERE {_tableInfo.PrimaryKey} = {_paramParser.AddParamIdentifier(_tableInfo.PrimaryKey)}";

        public SqlGenerator(TableInfo tableInfo, INamingConvention namingConvention)
        {
            Initialize(tableInfo, namingConvention);
        }

        public SqlGenerator(string tableName, string schema, INamingConvention namingConvention)
        {
            var tableInfo = new TableInfo(tableName, schema);
            tableInfo.FullTableName = Utils.GetSchemaQualifiedName(tableInfo.TableName,tableInfo.Schema);
            Initialize(tableInfo, namingConvention);

        }

        private void Initialize(TableInfo tableInfo, INamingConvention namingConvention)
        {
            _tableInfo = tableInfo;
            _namingConvention = namingConvention;
            _paramParser = new ParamParser(namingConvention);

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
            var columnNameList = _tableInfo.UpsertableColumns.Select(x => x.ColumnName);
            var sql =
                $"INSERT INTO {_tableInfo.FullTableName} ({_tableInfo.UpsertableColumnsString}) VALUES ({_paramParser.ConvertToParamNames(columnNameList)})";
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
            var setString = _paramParser.GetSetStringForUpdateQuery(columnNameList);
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

        public string GetStoredProcQuery(object prms = null)
        {

            return $"SELECT * FROM {_tableInfo.FullTableName}({_paramParser.GetStoredProcParamsNameValuesOnlySql(prms)})";
        }

        public string GetStoredProcCountQuery(object prms = null)
        {
            return $"SELECT COUNT(*) FROM {_tableInfo.FullTableName}({_paramParser.GetStoredProcParamsNameValuesOnlySql(prms)});";
        }
        #endregion
        

    }

}

