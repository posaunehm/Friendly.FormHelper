using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    public class TypeWrapper
    {
        private Dictionary<Type, TypeWrapper> _dict;


        private const string ClassNamePlaceHolder = "IGeneratedCloneFor";

        private readonly Type _baseType;
        private readonly string _wrappingName;

        public TypeWrapper(Type baseType, Dictionary<Type, TypeWrapper> dict)
        {
            _baseType = baseType;
            _dict = dict;
            _wrappingName = MakeCloneInterfaceName(_baseType);
        }

        public IEnumerable<string> PropertyStrings
        {
            get
            {
                foreach (var property in _baseType.GetProperties())
                {
                    var getterStr = property.CanRead ? "get;" : "";
                    var setterStr = property.CanWrite ? "set;" : "";
                    var typeName = GetWrappedTypeName(property.PropertyType);

                    if (property.GetIndexParameters().Length > 0)
                    {
                        var indexerStr = property.GetIndexParameters()
                            .Select(
                                (para, index) =>
                                    para.ParameterType.FullName.Replace('+', '.') + " index" + index.ToString() + ",")
                            .Reverse()
                            .Aggregate("", (ele, acc) => acc + ele).TrimEnd(',');
                        yield return
                            string.Format("{0} this[{1}] {{ {2} {3} }}", typeName, indexerStr, getterStr, setterStr);
                    }
                    else
                    {

                        yield return
                            string.Format("{0} {1} {{ {2} {3} }}", typeName, property.Name, getterStr, setterStr);
                    }
                }
            }
        }

        public string WrappingName
        {
            get { return _wrappingName; }
        }

        private static string MakeCloneInterfaceName(Type type)
        {
            if (!type.IsGenericType) return ClassNamePlaceHolder + type.Name.Replace("[]", "Array");

            var genericParamString = GetGenericParamString(Enumerable.Range(0, type.GetGenericArguments().Length).Select(i => string.Format("T{0}", i)));
            var typeNameWithoutGenericParams = type.Name.Split('`')[0];

            return string.Format("{0}{1}<{2}>", ClassNamePlaceHolder, typeNameWithoutGenericParams, genericParamString);
        }

        private static string GetGenericParamString(IEnumerable<string> typeParamString)
        {
            var genericParamString = typeParamString
                .Aggregate("", (s, i) => string.Format("{0}{1},", s, i))
                .TrimEnd(',');
            return genericParamString;
        }

        private string GetWrappingNameWithType(Type[] types)
        {
            if (!_baseType.IsGenericType) return ClassNamePlaceHolder + _baseType.Name;

            var genericParamString = GetGenericParamString(types.Select(GetParameterName));
            var typeNameWithoutGenericParams = _baseType.Name.Split('`')[0];

            return string.Format("{0}{1}<{2}>", ClassNamePlaceHolder, typeNameWithoutGenericParams, genericParamString);
        }

        private string GetParameterName(Type type)
        {
            return string.IsNullOrEmpty(type.FullName)
                ? string.Format("T{0}", type.GenericParameterPosition)
                : _dict[type].WrappingName;
        }

        string GetWrappedTypeName(Type type)
        {
            var propRealType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (type.IsArray)
            {
                propRealType = typeof(IArrayDummy<>);
                type = propRealType.MakeGenericType(new[] { type.GetElementType() });
            }

            var propTypeName = _dict.ContainsKey(propRealType) ? _dict[propRealType].GetWrappingNameWithType(type.GenericTypeArguments) : propRealType.FullName;

            return propTypeName ?? "T0";//(propRealType.IsGenericParameter ? string.Format("T{0}", propRealType.GenericParameterPosition) : "T0");
        }
    }
}