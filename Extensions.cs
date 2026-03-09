using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using EdmontonDrawingValidator.Model;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EdmontonDrawingValidator
{
    public static class Extensions
    {
        public static T DeepClone<T>(this T obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }

        public static T DeepClone2<T>(T obj)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(obj));
            }

            if (ReferenceEquals(obj, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();

            using (stream)
            {
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        
    }
    

    public static class StringExtension
    {
        public static bool IsEquals(this string str1, string str2)
        {
            if (str1 == null && str2 == null)
                return true;

            if (str1 == null || str2 == null)
                return false;

            return string.Compare(str1.Trim(), str2.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static string ToTitleCase(this string str)
        {
            if (str == null)
                return null;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }
    }



    //public static class ObjectCloner
    //{
    //    public static T DeepClone<T>(T obj)
    //    {
    //        if (obj == null)
    //            throw new ArgumentNullException(nameof(obj));

    //        return (T)DeepCloneObject(obj, new Dictionary<object, object>(new ReferenceEqualityComparer()));
    //    }

    //    private static object DeepCloneObject(object obj, IDictionary<object, object> visited)
    //    {
    //        if (obj == null)
    //            return null;

    //        Type type = obj.GetType();

    //        if (type.IsPrimitive || type.IsValueType || type == typeof(string) || type == typeof(decimal))
    //            return obj;

    //        if (visited.ContainsKey(obj))
    //            return visited[obj];

    //        if (obj is ICloneable cloneable)
    //            return cloneable.Clone();

    //        if (obj is Array array)
    //        {
    //            Array clonedArray = array.Clone() as Array;
    //            visited[obj] = clonedArray;

    //            for (int i = 0; i < array.Length; i++)
    //            {
    //                clonedArray.SetValue(DeepCloneObject(array.GetValue(i), visited), i);
    //            }

    //            return clonedArray;
    //        }

    //        if (obj is ICollection collection)
    //        {
    //            Type collectionType = obj.GetType();
    //            ICollection clonedCollection = (ICollection)Activator.CreateInstance(collectionType);

    //            visited[obj] = clonedCollection;

    //            foreach (object item in collection)
    //            {
    //                clonedCollection.Add(DeepCloneObject(item, visited));
    //            }

    //            return clonedCollection;
    //        }

    //        object cloneInstance = Activator.CreateInstance(type);
    //        visited[obj] = cloneInstance;

    //        foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
    //        {
    //            object fieldValue = fieldInfo.GetValue(obj);
    //            fieldInfo.SetValue(cloneInstance, DeepCloneObject(fieldValue, visited));
    //        }

    //        foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
    //        {
    //            if (propertyInfo.CanRead && propertyInfo.CanWrite)
    //            {
    //                object propertyValue = propertyInfo.GetValue(obj);
    //                propertyInfo.SetValue(cloneInstance, DeepCloneObject(propertyValue, visited));
    //            }
    //        }

    //        return cloneInstance;
    //    }

    //    private class ReferenceEqualityComparer : IEqualityComparer
    //    {
    //        public new bool Equals(object x, object y) => ReferenceEquals(x, y);

    //        public int GetHashCode(object obj) => obj.GetHashCode();
    //    }
    //}
}
