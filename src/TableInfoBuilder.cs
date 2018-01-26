using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using NDbPortal.Names;
using NDbPortal.Names.MappingAttributes;

namespace NDbPortal
{
    public class TableInfoBuilder<T> : ITableInfoBuilder<T>
    {
        private readonly INamingConvention _namingConvention;
        private readonly TableInfo _tableInfo = new TableInfo();
        private readonly TypeInfo _t;
        private readonly DbOptions _dbOptions;

        public TableInfoBuilder(INamingConvention namingConvention, DbOptions dbOptions)
        {
            _namingConvention = namingConvention;
            _dbOptions = dbOptions;
            _t = typeof(T).GetTypeInfo();
        }
        public ITableInfoBuilder<T> SetPrimaryKey()
        {
            _tableInfo.PrimaryKey = ReflectionUtilities.GetPrimaryKey(_t) ?? _dbOptions.DefaultPrimaryKeyName;
            return this;
        }

        public ITableInfoBuilder<T> SetTableName()
        {
            var a = _t.GetCustomAttributes<TableAttribute>(true).FirstOrDefault();
            _tableInfo.TableName = GetTableName(a);
            _tableInfo.Schema = GetSchemaName(a);
            _tableInfo.FullTableName = Utils.GetSchemaQualifiedName(_tableInfo.TableName, _tableInfo.Schema);
            return this;
        }

        private string GetSchemaName(TableAttribute tableAttribute)
        {
            if (!string.IsNullOrWhiteSpace(tableAttribute?.Schema))
            {
                return tableAttribute?.Schema;
            }
            if (!string.IsNullOrWhiteSpace(_dbOptions.DefaultSchema))
            {
                return _dbOptions.DefaultSchema;
            }
            return _namingConvention.ConvertToDbName(_t.Namespace.Split('.').LastOrDefault());
        }

        private string GetTableName(TableAttribute tableAttribute)
        {
            return !string.IsNullOrWhiteSpace(tableAttribute?.Name) ? tableAttribute?.Name : _namingConvention.ConvertToDbName(_t.Name);
        }

        public ITableInfoBuilder<T> SetColumnInfos()
        {
            var properties = _t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            _tableInfo.Columns = GetColumnInfos(properties);
            _tableInfo.UpsertableColumns = GetInsertUpdatableColumns(_tableInfo.Columns);
            return this;
        }

        public TableInfo Build()
        {
            return _tableInfo;
        }

        #region Privates
        private IEnumerable<ColumnInfo> GetInsertUpdatableColumns(IList<ColumnInfo> columns)
        {
            var retColumns = new List<ColumnInfo>();
            var notDisplayColumns = columns.Where(x => !x.IsDisplayColumn);
            foreach (var columnInfo in notDisplayColumns)
            {
                if (!columnInfo.ColumnName.Equals(ReflectionUtilities.GetPrimaryKey(_t), StringComparison.OrdinalIgnoreCase))
                {
                    retColumns.Add(columnInfo);
                }
            }
            return retColumns;
        }


        private IList<ColumnInfo> GetColumnInfos(PropertyInfo[] properties)
        {
            var columns = new List<ColumnInfo>();
            foreach (PropertyInfo property in properties)
            {
                if (property.HasAttribute<IgnoreAttribute>())
                {
                    continue;
                }

                columns.Add(GetColumnInfo(property));
            }
            return columns;
        }

        private ColumnInfo GetColumnInfo(PropertyInfo property)
        {
            var columnName = property.GetCustomAttribute<ColumnAttribute>(true);
            var columnInfo = new ColumnInfo
            {
                ColumnName = columnName == null ? _namingConvention.ConvertToDbName(property.Name) : columnName.ColumnName,
                IsDisplayColumn = property.HasAttribute<DisplayColumnAttribute>()
            };
            return columnInfo;
        }

        #endregion
    }
}
