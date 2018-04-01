using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using NDbPortal.Names;
using NDbPortal.Names.MappingAttributes;

namespace NDbPortal
{
    public class SqlGenerator<T> : ISqlGenerator<T>
    {
        private readonly IParamParser _paramParser;
        private readonly DbOptions _dbOptions;

        public SqlGenerator(INamingConvention namingConvention, DbOptions dbOptions)
        {
            _paramParser = new ParamParser(namingConvention);
            NamingConvention = namingConvention;
            _dbOptions = dbOptions;
            TableInfo = GetTableInfo();

        }
        public SqlGenerator(INamingConvention namingConvention, IOptions<DbOptions> dbOptions) : this(namingConvention, dbOptions.Value)
        {

        }

        #region Public Members
        public TableInfo TableInfo { get; private set; }
        public INamingConvention NamingConvention { get; private set; }

        /// <summary>
        /// Generest SELECT * query
        /// </summary>
        /// <returns></returns>
        public string GetSelectAllQuery()
        {
            return $"SELECT * FROM {TableInfo.FullTableName};";
        }

        /// <summary>
        /// Generest a sql query that SELECTs by id. SQL Parameter id
        /// </summary>
        /// <returns></returns>
        public string GetSelectByIdQuery()
        {
            return $"SELECT * FROM {TableInfo.FullTableName} {GetWhereId()};";
        }

        public string GetSelectByColumnNameQuery(string columnName)
        {
            return $"SELECT * FROM {TableInfo.FullTableName} WHERE LOWER({columnName} ::text) = LOWER(@{columnName} ::text);";
        }

        /// <summary>
        /// Creates an insert query. Parameters column values
        /// </summary>
        /// <returns></returns>
        public string GetInsertQuery()
        {
            var columnNameList = TableInfo.UpsertableColumns.Select(x => x.ColumnName);
            var sql =
                $"INSERT INTO {TableInfo.FullTableName} ({TableInfo.UpsertableColumnsString}) VALUES ({_paramParser.ConvertToParamNames(columnNameList)})";
            return sql + $" RETURNING {TableInfo.PrimaryKey};";
        }

        public string GetDeleteQuery()
        {
            return $"DELETE FROM {TableInfo.FullTableName} {GetWhereId()};";
        }

        /// <summary>
        /// Creates a Update query
        /// </summary>
        /// <returns>sql query</returns>
        public string GetUpdateQuery()
        {
            var columnNameList = TableInfo.Columns.Select(x => x.ColumnName).ToList();
            var setString = _paramParser.GetSetStringForUpdateQuery(columnNameList);
            var sql = $"UPDATE {TableInfo.FullTableName} SET {setString} {GetWhereId()};";
            return sql;

            ;
        }

        public string GetPagedQuery(int page, string orderByColumnName)
        {
            return $"SELECT * FROM {TableInfo.FullTableName} ORDER BY {orderByColumnName} DESC LIMIT {_dbOptions.PagedListSize} OFFSET {GetOffset(page, _dbOptions.PagedListSize)}";
        }

        public string GetCountQuery()
        {
            return $"SELECT COUNT(*) FROM {TableInfo.FullTableName};";
        }
        #endregion

        #region Privates

        private string GetWhereId()
        {
            return $" WHERE {TableInfo.PrimaryKey} = {_paramParser.AddParamIdentifier(TableInfo.PrimaryKey)}";
        }
        private TableInfo GetTableInfo()
        {
            return new TableInfoBuilder<T>(NamingConvention, _dbOptions)
                .SetTableName()
                .SetPrimaryKey()
                .SetColumnInfos().Build();
        }

        private long GetOffset(int page, int pageSize)
        {
            return page * pageSize;
        }


        #endregion

    }

}

