using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace NDbPortal
{
    public static class Mapper
    {
        /// <summary>
        /// Gets the T Object from the command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="dispose">dispose the command</param>
        /// <returns>an instance of T</returns>
        public static T GetObject<T>(IDbCommand command, bool dispose = true)
        {
            IDataReader rdr = null;
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();
                var hasRows = false;
                T t = default(T);
                using (rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        hasRows = true;
                        t = GetSingleObject<T>(rdr);
                        rdr.Dispose();
                        break;
                    }

                }
                return hasRows ? t : default(T);
            }
            finally
            {
                if (dispose)
                {
                    Dispose(command, rdr);

                }
            }
        }

        /// <summary>
        /// Gets a list of T Objects from the command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="dispose">dispose cmd object</param>
        /// <returns>List of instance of T objects</returns>
        public static IEnumerable<T> GetObjects<T>(IDbCommand command, bool dispose = true)
        {
            IDataReader rdr = null;
            try
            {
                var tList = new List<T>();
                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();
                using (rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        tList.Add(GetSingleObject<T>(rdr));
                    }
                }
                return tList;
            }
            finally
            {
                if (dispose)
                {
                    Dispose(command, rdr);
                }
            }
        }

        public static List<KeyValuePair<TKey, TValue>> GetKeyValuePairs<TKey, TValue>(IDbCommand cmd)
        {
            IDataReader rdr = null;
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                var keyValPairs = new List<KeyValuePair<TKey, TValue>>();
                using (rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        keyValPairs.Add(new KeyValuePair<TKey, TValue>(rdr.GetValue(0).GetCastedObject<TKey>(),
                            rdr.GetValue(1).GetCastedObject<TValue>()));
                    }
                }
                return keyValPairs;

            }
            finally
            {
                Dispose(cmd, rdr);

            }
        }

        public static T ExecuteScalar<T>(IDbCommand cmd, bool dispose = true)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                return ConvertTo<T>(cmd.ExecuteScalar());
            }
            finally
            {
                if (dispose)
                {
                    Dispose(cmd);
                }
            }

        }

        public static int ExecuteNonQuery(IDbCommand cmd)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                Dispose(cmd);
            }
        }

        #region Privates

        private static List<PropertyInfo> GetOrderPropertyInfos(IEnumerable<string> columnNames,
           List<PropertyInfo> propertyInfo)
        {
            var orderedPropertyInfos = new List<PropertyInfo>();
            foreach (var name in columnNames)
            {

                var info = GetPropertyInfoForOrderedList(propertyInfo, name);

                if (info == null)
                {
                    throw new Exception(
                        $"No matching member for column {name} Make sure the column name has a matching member according to the convention or attribute specified.");
                }
                orderedPropertyInfos.Add(info);
            }
            return orderedPropertyInfos;
        }

        private static PropertyInfo GetPropertyInfoForOrderedList(List<PropertyInfo> propertyInfos, string colunmName)
        {
            return propertyInfos.FirstOrDefault(info =>
                    string.Equals(info.Name, ProcessColumnName(colunmName), StringComparison.CurrentCultureIgnoreCase));
        }

        private static string ProcessColumnName(string colunmName)
        {
            if (colunmName.Contains('_'))
            {
                return colunmName.Replace("_", "");
            }
            return colunmName;
        }

        private static void SetProperties<T>(IDataRecord rdrRow, T t, List<string> columnNames,
            List<PropertyInfo> orderedPropertyInfos)
        {
            if (rdrRow == null) throw new ArgumentNullException(nameof(rdrRow));
            for (var i = 0; i < columnNames.Count; i++)
            {
                var propertyInfo = orderedPropertyInfos[i];
                var value = rdrRow.GetValue(i);
                var propType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                var safeValue = ConvertToSafeValue(value, propType);
                propertyInfo.SetValue(t, safeValue, null);
            }
        }

        private static T ConvertTo<T>(object value)
        {
            var type = typeof(T);
            return (T)ConvertToSafeValue(value, type);

        }

        private static object ConvertToSafeValue(object value, Type type)
        {
            if (type == typeof(Enum))
            {
                return (value == null || value == DBNull.Value) ? null : Enum.Parse(type, value.ToString());
            }
            else
            {
                return (value == null || value == DBNull.Value) ? null : Convert.ChangeType(value, type);
            }

        }

        private static T GetCastedObject<T>(this object val)
        {
            var propType = Nullable.GetUnderlyingType(val.GetType()) ?? typeof(T);
            var safeValue = (val == DBNull.Value) ? null : Convert.ChangeType(val, propType);
            if (safeValue != null)
            {
                return (T)safeValue;
            }
            return default(T);
        }

        private static T GetSingleObject<T>(IDataRecord rdr)
        {
            if (typeof(T).GetTypeInfo().IsValueType)
            {
                return rdr.GetValue(0).GetCastedObject<T>();
            }
            var t = Activator.CreateInstance<T>();


            var columnNames = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToList();
            var properties = typeof(T).GetProperties();
            var orderedPropertyInfos = GetOrderPropertyInfos(columnNames, properties.ToList());

            if (columnNames.Count != orderedPropertyInfos.Count)
                throw new Exception(
                    "Not all columns are mapped to object properties. Please Make sure that all the columns have thier respective properties or that they follow the specified naming conventions or that their names are not misspelled.");
            SetProperties(rdr, t, columnNames, orderedPropertyInfos);
            return t;
        }

        private static void Dispose(IDbCommand cmd, IDataReader rdr = null)
        {
            cmd.Connection.Close();
            cmd.Connection.Dispose();
            cmd.Dispose();
            rdr?.Dispose();
        }

        #endregion
    }
}