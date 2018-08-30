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
        public static T GetObject<T>(IDbCommand command)
        {
            IDataReader rdr = null;
            try
            {
                if (command.Connection.State == ConnectionState.Closed)
                {
                    command.Connection.Open();
                }

                bool hasRows = false;
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
                Dispose(rdr);
            }
        }

        /// <summary>
        /// Gets a list of T Objects from the command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns>List of instance of T objects</returns>
        public static IEnumerable<T> GetObjects<T>(IDbCommand command)
        {
            IDataReader rdr = null;
            try
            {
                List<T> tList = new List<T>();
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }

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
                Dispose(rdr);
            }
        }

        public static List<KeyValuePair<TKey, TValue>> GetKeyValuePairs<TKey, TValue>(IDbCommand cmd)
        {
            IDataReader rdr = null;
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                List<KeyValuePair<TKey, TValue>> keyValPairs = new List<KeyValuePair<TKey, TValue>>();
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
                Dispose(rdr);

            }
        }

        public static T ExecuteScalar<T>(IDbCommand cmd)
        {
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }

            return ConvertTo<T>(cmd.ExecuteScalar());


        }

        public static int ExecuteNonQuery(IDbCommand cmd)
        {
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }

            return cmd.ExecuteNonQuery();

        }

        #region Privates

        private static List<PropertyInfo> GetOrderPropertyInfos(IEnumerable<string> columnNames,
           List<PropertyInfo> propertyInfo)
        {
            List<PropertyInfo> orderedPropertyInfos = new List<PropertyInfo>();
            foreach (string name in columnNames)
            {

                PropertyInfo info = GetPropertyInfoForOrderedList(propertyInfo, name);

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
            if (rdrRow == null)
            {
                throw new ArgumentNullException(nameof(rdrRow));
            }

            for (int i = 0; i < columnNames.Count; i++)
            {
                PropertyInfo propertyInfo = orderedPropertyInfos[i];
                object value = rdrRow.GetValue(i);
                Type propType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                object safeValue = ConvertToSafeValue(value, propType);
                propertyInfo.SetValue(t, safeValue, null);
            }
        }

        private static T ConvertTo<T>(object value)
        {
            if (value == null)
            {
                return default(T);
            }

            Type type = typeof(T);
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
            Type propType = Nullable.GetUnderlyingType(val.GetType()) ?? typeof(T);
            object safeValue = (val == DBNull.Value) ? null : Convert.ChangeType(val, propType);
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
            T t = Activator.CreateInstance<T>();


            List<string> columnNames = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToList();
            PropertyInfo[] properties = typeof(T).GetProperties();
            List<PropertyInfo> orderedPropertyInfos = GetOrderPropertyInfos(columnNames, properties.ToList());

            if (columnNames.Count != orderedPropertyInfos.Count)
            {
                throw new Exception(
                    "Not all columns are mapped to object properties. Please Make sure that all the columns have thier respective properties or that they follow the specified naming conventions or that their names are not misspelled.");
            }

            SetProperties(rdr, t, columnNames, orderedPropertyInfos);
            return t;
        }

        private static void Dispose(IDataReader rdr = null)
        {
            rdr?.Dispose();
        }
        #endregion
    }
}