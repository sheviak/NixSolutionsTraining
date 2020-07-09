using ORM.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ORM.Helpers
{
    public class Helper
    {
        public static string GetTableName<T>(T item)
        {
            var table = (Attribute.GetCustomAttribute(item.GetType(), typeof(TableAttribute)) as TableAttribute)?.Name;
            return table;
        }

        public static string GetTableName(Type type)
        {
            var table = (type.GetCustomAttribute(typeof(TableAttribute), false) as TableAttribute)?.Name;
            return table;
        }

        public static bool IsNullOrEmptyCollection<T>(IEnumerable<T> collection) => collection == null || !collection.Any();

        public static List<PropertyInfo> GetOneToOneProperties(Type item)
        {
            var properties = item
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(OneToOneAttribute)))
                .ToList();

            return properties;
        }

        public static IList<PropertyInfo> GetCollectionOneToManyProperties(Type item)
        {
            var properties = item
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(OneToManyAttribute)))
                .ToList();

            return properties;
        }

        public static IList<PropertyInfo> GetCollectionManyToManyProperties(Type item)
        {
            var properties = item
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(ManyToManyAttribute)))
                .ToList();

            return properties;
        }

        public static List<PropertyInfo> GetModelForeignKey<T>(T item)
        {
            var fk = item
                .GetType()
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(FKAttribute)))
                .ToList();

            return fk;
        }

        public static PropertyInfo GetPrimaryKeyProperties<T>(T item)
        {
            return item
                .GetType()
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(PKAttribute)))
                .FirstOrDefault();
        }

        public static IEnumerable<PropertyInfo> GetMemberProperties<T>(T item)
        {
            return item
                .GetType()
                .GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(MemberAttribute)) &&
                            !Attribute.IsDefined(x, typeof(OneToManyAttribute)) &&
                            !Attribute.IsDefined(x, typeof(FKAttribute)) &&
                            !Attribute.IsDefined(x, typeof(PKAttribute)))
                .ToList();
        }

        public static List<T> ConvertToObject<T>(DataColumnCollection col, DataRowCollection row) where T : class, new()
        {
            var accessor = Activator.CreateInstance(typeof(T));
            var properties = accessor.GetType().GetProperties().ToList();
            var list = new List<T>();

            for (int i = 0; i < row.Count; i++)
            {
                var t = new T();

                for (int j = 0; j < col.Count; j++)
                {
                    var fieldName = col[j].ColumnName;

                    if (properties.Any(m => string.Equals(m.Name, fieldName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var prop = properties.First(m => string.Equals(m.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                        prop.SetValue(t, row[i].ItemArray.GetValue(j));
                    }
                }

                list.Add(t);
            }

            return list;
        }
    }
}
