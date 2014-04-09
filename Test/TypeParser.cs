using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    public class TypeParser
    {
        public static void GetTypeRec(Type type, Dictionary<Type, TypeWrapper> dict)
        {
            if ((type.IsValueType && !type.IsGenericType) || type == typeof(string) || type.IsByRef || type.IsGenericParameter) { return; }

            var realType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (realType.IsArray)
            {
                realType = typeof(IArrayDummy<>);
            }

            if (dict.ContainsKey(realType)) { return; }

            dict.Add(realType, new TypeWrapper(realType, dict));

            foreach (var propType in type.GetProperties().Select(info => info.PropertyType))
            {
                GetTypeRec(propType, dict);
            }
            foreach (var method in type.GetMethods())
            {
                GetTypeRec(method.ReturnType, dict);
            }
        }
    }
}