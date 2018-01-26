using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NDbPortal.Names;
using NDbPortal.Names.MappingAttributes;

namespace NDbPortal
{
    public static class ReflectionUtilities
    {
        public static TableAttribute GetEntityAttribute(Type t)
        {
            var entityAttr = t.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (entityAttr != null)
            {
                return entityAttr;
            }
            else
            {
                throw new Exception("Could not find table attribute on " + t);
            }

        }


        public static string GetPropertyNameFromExpression<T>(Expression<Func<T, bool>> expression)
        {
            if (expression.Body is MemberExpression)
            {
                return ((MemberExpression)(expression.Body)).Member.Name;
            }

            else if (expression.Body is BinaryExpression)
            {
                var bin = (BinaryExpression)expression.Body;
                return ((MemberExpression)bin.Left).Member.Name;


            }
            else
            {
                throw new Exception("Property parsing unsuccessful in" + typeof(ReflectionUtilities) + "Expression may not be a binary expression");

            }
        }
        public static object GetValueFromExpression<T>(Expression<Func<T, bool>> expression)
        {
            if (expression.Body is BinaryExpression)
            {
                var bin = (BinaryExpression)expression.Body;
                if (bin.Right != null)
                {
                    if (bin.Right is MemberExpression)
                    {
                        var right = (MemberExpression)bin.Right;
                        return Expression.Lambda(right).Compile().DynamicInvoke();
                    }
                    if (bin.Right is UnaryExpression)
                    {
                        var right = (UnaryExpression)bin.Right;
                        return Expression.Lambda(right).Compile().DynamicInvoke();
                    }

                }
            }
            else
            {
                var bin = (UnaryExpression)expression.Body;
                var right = (MemberExpression)bin.Operand;
                return Expression.Lambda(right).Compile().DynamicInvoke();
            }

            throw new NotImplementedException("No implementation for Unary Expression or other expression types yet.");

        }

        public static object MergeObjects(object obj1, object obj2)
        {
            var retObject = new ExpandoObject();
            foreach (PropertyInfo property in obj1.GetType().GetProperties())
            {
                ((IDictionary<string, object>)retObject)[property.Name] = property.GetValue(obj1);
            }
            foreach (PropertyInfo property in obj2.GetType().GetProperties())
            {
                ((IDictionary<string, object>)retObject)[property.Name] = property.GetValue(obj2);
            }

            return retObject;
        }

        public static TProperty GetPropertyValue<TObject, TProperty>(string propertyName, TObject obj)
        {
            if (obj is IDynamicMetaObjectProvider)
            {
                var propertyValues = (IDictionary<string, object>)obj;
                return CastTo<TProperty>(propertyValues[propertyName]);
            }
            else
            {
                if (obj == null || string.IsNullOrWhiteSpace(propertyName)) return default(TProperty);
                var properties = obj.GetType().GetProperties();
                var first = default(TProperty);
                var propertyFound = false;
                foreach (var propertyInfo in properties)
                {
                    if (string.Equals(propertyInfo.Name, propertyName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        first = CastTo<TProperty>(propertyInfo.GetValue(obj, null));
                        propertyFound = true;
                        break;
                    }
                }
                if (!propertyFound)
                {
                    throw new Exception($"Property {propertyName} not found");
                }
                return first;
            }

        }

        public static IEnumerable<string> GetProperties(object obj)
        {
            if (obj is IDynamicMetaObjectProvider)
            {
                return ((IDictionary<string, object>)obj).Keys;

            }
            else
            {
                return obj.GetType().GetProperties().Select(x => x.Name);
            }
        }

        public static bool HasAttribute<T>(this MemberInfo type) where T : Attribute
        {
            var attrs = new List<T>();
            attrs.AddRange(type.GetCustomAttributes<T>(true));
            if (type.DeclaringType.GetInterfaces().Any())
            {
                var intProp = type.DeclaringType.GetInterfaces().Select(i => i.GetProperty(type.Name)).Distinct().ToList();
                if (intProp.Any())
                {
                    foreach (var propertyInfo in intProp)
                    {
                        if (propertyInfo != null)
                        {
                            attrs.AddRange(propertyInfo.GetCustomAttributes<T>());
                        }
                    }
                }
            }

            return attrs.Distinct().Any();
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance) != null;
        }

        public static string GetPrimaryKey<T>(T t)
        {
            return GetPrimaryKey(typeof(T).GetTypeInfo());
        }

        public static string GetPrimaryKey(TypeInfo t)
        {
            var priamryKeyAttr = t.GetCustomAttributes<PrimaryKeyAttribute>(true).ToList();
            return priamryKeyAttr.Count > 0 ? priamryKeyAttr[0].Value : null;
        }

        public static T CastTo<T>(object input)
        {
            return (T)Convert.ChangeType(input, typeof(T));
        }

    }
}
