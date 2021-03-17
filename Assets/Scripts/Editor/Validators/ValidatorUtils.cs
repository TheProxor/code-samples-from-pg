using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Drawmasters.Editor
{

    public static class ValidatorUtils
    {
        public static T[] GetAllFieldsKeys<T>(this Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static) 
        {
            List<T> result = new List<T>();

            result.AddRange(type
                            .GetFields(bindingFlags)
                            .Where(f => f.FieldType == typeof(T))
                            .Select(f => (T)f.GetValue(null)));

            foreach (Type nestedType in type.GetNestedTypes())
            {
                result.AddRange(nestedType.GetAllFieldsKeys<T>(bindingFlags));
            }

            return result.ToArray();
        }
    }
}
