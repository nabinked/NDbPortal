using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NDbPortal.Names;
using NDbPortal.Names.MappingAttributes;

namespace NDbPortal
{
    public class TableInfoBuilder<T> : ITableInfoBuilder<T>
    {
        private readonly INamingConvention _namingConvention;
        private readonly TableInfo _tableInfo = new TableInfo();
        private readonly TypeInfo _t;

        public TableInfoBuilder(INamingConvention namingConvention)
        {
            _namingConvention = namingConvention;
            _t = typeof(T).GetTypeInfo();
        }
        public ITableInfoBuilder<T> SetPrimaryKey()
        {
            _tableInfo.PrimaryKey = ReflectionUtilities.GetPrimaryKey(_t);
            return this;
        }

        public ITableInfoBuilder<T> SetTableName()
        {
            var a = _t.GetCustomAttributes<TableAttribute>(true).ToArray();
            _tableInfo.TableName = a.Length == 0 ? _namingConvention.ConvertToDbName(_t.Name) : a[0]?.Name;
            _tableInfo.TableSchema = a.Length == 0 ? _namingConvention.ConvertToDbName(_t.Namespace.Split('.').LastOrDefault()) : a[0]?.Schema;
            _tableInfo.FullTableName = $"{_tableInfo.TableSchema}.{_tableInfo.TableName}";
            return this;
        }

        public ITableInfoBuilder<T> SetColumnInfos()
        {
            var columns = new List<ColumnInfo>();
            var properties = _t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.HasAttribute<IgnoreAttribute>())
                {
                    continue;
                }
                var columnName = property.GetCustomAttribute<ColumnAttribute>(true);
                var columnInfo = new ColumnInfo
                {
                    ColumnName =
                        columnName == null ? _namingConvention.ConvertToDbName(property.Name) : columnName.ColumnName,
                    IsDisplayColumn = property.HasAttribute<DisplayColumnAttribute>()
                };

                columns.Add(columnInfo);
            }
            _tableInfo.Columns = columns;
            _tableInfo.InsertUpdateColumns = GetInsertUpdatableColumns(columns);
            return this;
        }

        private IEnumerable<ColumnInfo> GetInsertUpdatableColumns(List<ColumnInfo> columns)
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

        public TableInfo Build()
        {
            return _tableInfo;
        }
    }
}
