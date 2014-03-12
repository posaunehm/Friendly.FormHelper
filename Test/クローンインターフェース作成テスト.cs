using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class クローンインターフェース作成テスト
    {
        private Dictionary<Type, TypeWrapper> _dict;
        private const string ClassNamePlaceHolder = "IGeneratedCloneFor";


        [TestInitialize]
        public void Setup()
        {
            _dict = new Dictionary<Type, TypeWrapper>();
        }


        [TestMethod]
        public void Formのクローンインターフェースを作成する()
        {
            var formType = typeof (Form);

            GetTypeRec(formType,_dict);
        }

        [TestMethod]
        public void 簡単な型のクローンインターフェースを作成する()
        {
            var formType = typeof(Testclass);

            GetTypeRec(formType, _dict);

            _dict.ContainsKey(typeof (Testclass)).IsTrue();
            _dict.ContainsKey(typeof (List<>)).IsTrue();
        }

        [TestMethod]
        public void 配列のクローンインタフェースを作成する()
        {
            var type = typeof(int[]);

            GetTypeRec(type, _dict);

            _dict.ContainsKey(typeof(int[])).IsFalse();
            _dict.ContainsKey(typeof(IArrayDummy<>)).IsTrue();
        }

        [TestMethod]
        public void 束縛する型が異なるジェネリック型を二つ加えても片方しか追加されない()
        {
            GetTypeRec(typeof(TestGeneric<string, int>), _dict);
            GetTypeRec(typeof(TestGeneric<int,object>), _dict);

            _dict.ContainsKey(typeof(TestGeneric<,>)).IsTrue();
            _dict.ContainsKey(typeof(TestGeneric<string,int>)).IsFalse();
            _dict.ContainsKey(typeof(TestGeneric<int,object>)).IsFalse();
        }

        [TestMethod]
        public void 返り値がGenericの場合()
        {
            GetTypeRec(typeof(IEnumerator<>), _dict);
            _dict.Keys.Any(type => type.IsGenericParameter).IsFalse();
        }

        [TestMethod]
        public void NullableがGeneric型として登録される()
        {
            GetTypeRec(typeof(int?), _dict);
            _dict.ContainsKey(typeof (Nullable<>)).IsTrue();
        }

        [TestMethod]
        public void 通常のClassのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof(Object)).WrappingName.Is("IGeneratedCloneForObject");
        }

        [TestMethod]
        public void GenericClassのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof (Dictionary<string, string>)).WrappingName.Is("IGeneratedCloneForDictionary<T0,T1>");
        }

        [TestMethod]
        public void Arrayのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof(object[])).WrappingName.Is("IGeneratedCloneForObjectArray");
        }

        [TestMethod]
        public void Nullableのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof(int?)).WrappingName.Is("IGeneratedCloneForNullable<T0>");
        }

        [TestMethod]
        public void Genric型に実際の型を入れる()
        {
            var type = typeof(TestGeneric<string,int>);
            new TypeWrapper(type).GetWrappingNameWithType(new[] { typeof(object), typeof(double) })
                .Is("IGeneratedCloneForTestGeneric<System.Object,System.Double>");
        }


        [TestMethod]
        public void ジェネリックパラメータを型に入れた時の型付ラッパ名をとるテスト()
        {
            var type = typeof(TestGeneric<string, int>);

            var genericParamType = typeof (Task<>).GetProperty("Result").PropertyType;

            new TypeWrapper(type).GetWrappingNameWithType(new []{genericParamType})
                .Is("IGeneratedCloneForTestGeneric<T0>");
        }

        [TestMethod]
        public void メソッドのうちからプロパティをラップしているメソッドを除く()
        {
            var methods = typeof (Testclass).GetMethods().Where(info => !info.IsSpecialName);

            methods.Any(info => info.Name.Contains("get_")).IsFalse();
        }

        [TestMethod]
        public void 型名を生成された型名に変換する()
        {
            GetTypeRec(typeof(Testclass), _dict);

            GetWrappedTypeName(typeof(TestClassSub), _dict).Is("IGeneratedCloneForTestClassSub");
        }

        [TestMethod]
        public void 全列挙したTypeすべてでGetWrappedTypeName()
        {
            GetTypeRec(typeof(Form), _dict);

            foreach (var type in _dict.Keys)
            {
                GetWrappedTypeName(type, _dict);

                foreach (var pInfo in type.GetProperties().Where(prop => !((prop.GetGetMethod() != null && prop.GetGetMethod().IsStatic) || (prop.GetSetMethod() != null && prop.GetSetMethod().IsStatic))))
                {
                    
                }
            }

            var menuStripType = typeof (MenuStrip).GetMethods().Where(info => !info.IsStatic && !info.IsSpecialName ).Distinct(new ToStringComparer<MethodInfo>()).ToArray();
        }


        class ToStringComparer<T> : EqualityComparer<T>
        {
            public override bool Equals(T x, T y)
            {
                return x.ToString() == y.ToString();
            }

            public override int GetHashCode(T obj)
            {
                return obj != null ? obj.ToString().GetHashCode() : 0;
            }
        }

        static void GetTypeRec(Type type, Dictionary<Type, TypeWrapper> dict)
        {
            if ((type.IsValueType && !type.IsGenericType) || type == typeof(string) || type.IsByRef || type.IsGenericParameter) { return; }

            var realType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (realType.IsArray)
            {
                realType = typeof(IArrayDummy<>);
            }

            if (dict.ContainsKey(realType)) { return; }

            dict.Add(realType, new TypeWrapper(realType));

            foreach (var propType in type.GetProperties().Select(info => info.PropertyType))
            {
                GetTypeRec(propType, dict);
            }
            //return;
            foreach (var method in type.GetMethods())
            {
                GetTypeRec(method.ReturnType, dict);
            }
        }

        static string GetWrappedTypeName(Type type, Dictionary<Type, TypeWrapper> dict)
        {
            var propRealType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (type.IsArray)
            {
                propRealType = typeof(IArrayDummy<>);
                type = propRealType.MakeGenericType(new[] { type.GetElementType() });
            }

            var propTypeName = dict.ContainsKey(propRealType) ? dict[propRealType].GetWrappingNameWithType(type.GenericTypeArguments) : propRealType.FullName;

            return propTypeName ?? (propRealType.IsGenericParameter ? string.Format("T{0}", propRealType.GenericParameterPosition) : "T0");
        }
    }



    class Testclass
    {
        public string Hoge { get; set; }

        public List<string> Fuga { get; set; }

        public TestClassSub Piyo { get { return null; } }
    }

    class TestClassSub
    {
        
    }

    class TestGeneric<T , S>
    {
    }


    class TestGenericReturn<T>
    {
        T Get { get; set; }
    }
    interface IArrayDummy<T>
    {

        System.Int32 Length { get; }
        System.Int64 LongLength { get; }
        System.Int32 Rank { get; }
        System.Object SyncRoot { get; }
        System.Boolean IsReadOnly { get; }
        System.Boolean IsFixedSize { get; }
        System.Boolean IsSynchronized { get; }
        void Set(System.Int32 index0, T index1);
        T Get(System.Int32 index0);
        System.Object GetValue(System.Int32[] index0);
        System.Object GetValue(System.Int32 index0);
        System.Object GetValue(System.Int32 index0, System.Int32 index1);
        System.Object GetValue(System.Int32 index0, System.Int32 index1, System.Int32 index2);
        System.Object GetValue(System.Int64 index0);
        System.Object GetValue(System.Int64 index0, System.Int64 index1);
        System.Object GetValue(System.Int64 index0, System.Int64 index1, System.Int64 index2);
        System.Object GetValue(System.Int64[] index0);
        void SetValue(System.Object index0, System.Int32 index1);
        void SetValue(System.Object index0, System.Int32 index1, System.Int32 index2);
        void SetValue(System.Object index0, System.Int32 index1, System.Int32 index2, System.Int32 index3);
        void SetValue(System.Object index0, System.Int32[] index1);
        void SetValue(System.Object index0, System.Int64 index1);
        void SetValue(System.Object index0, System.Int64 index1, System.Int64 index2);
        void SetValue(System.Object index0, System.Int64 index1, System.Int64 index2, System.Int64 index3);
        void SetValue(System.Object index0, System.Int64[] index1);
        System.Int32 GetLength(System.Int32 index0);
        System.Int64 GetLongLength(System.Int32 index0);
        System.Int32 GetUpperBound(System.Int32 index0);
        System.Int32 GetLowerBound(System.Int32 index0);
        System.Object Clone();
        void CopyTo(System.Array index0, System.Int32 index1);
        void CopyTo(System.Array index0, System.Int64 index1);
        System.Collections.IEnumerator GetEnumerator();
        void Initialize();
        System.String ToString();
        System.Boolean Equals(System.Object index0);
        System.Int32 GetHashCode();
        System.Type GetType();

    }

    class TypeWrapper
    {
        private const string ClassNamePlaceHolder = "IGeneratedCloneFor";

        private readonly Type _baseType;
        private readonly string _wrappingName;

        public TypeWrapper(Type baseType)
        {
            _baseType = baseType;
            _wrappingName = MakeCloneInterfaceName(_baseType);
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

        public string GetWrappingNameWithType(Type[] types)
        {
            if (!_baseType.IsGenericType) return ClassNamePlaceHolder + _baseType.Name.Replace("[]", "Array");

            var genericParamString = GetGenericParamString(types.Select(GetParameterName));
            var typeNameWithoutGenericParams = _baseType.Name.Split('`')[0];

            return string.Format("{0}{1}<{2}>", ClassNamePlaceHolder, typeNameWithoutGenericParams, genericParamString);
        }

        private static string GetParameterName(Type type)
        {
            return string.IsNullOrEmpty(type.FullName)
                ? string.Format("T{0}", type.GenericParameterPosition)
                : type.FullName;
        }
    }
}
