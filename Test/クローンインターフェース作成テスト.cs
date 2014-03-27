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

            TypeParser.GetTypeRec(formType,_dict);
        }

        [TestMethod]
        public void 簡単な型のクローンインターフェースを作成する()
        {
            var formType = typeof(Testclass);

            TypeParser.GetTypeRec(formType, _dict);

            _dict.ContainsKey(typeof (Testclass)).IsTrue();
            _dict.ContainsKey(typeof (List<>)).IsTrue();
        }

        [TestMethod]
        public void 配列のクローンインタフェースを作成する()
        {
            var type = typeof(int[]);

            TypeParser.GetTypeRec(type, _dict);

            _dict.ContainsKey(typeof(int[])).IsFalse();
            _dict.ContainsKey(typeof(IArrayDummy<>)).IsTrue();
        }

        [TestMethod]
        public void 束縛する型が異なるジェネリック型を二つ加えても片方しか追加されない()
        {
            TypeParser.GetTypeRec(typeof(TestGeneric<string, int>), _dict);
            TypeParser.GetTypeRec(typeof(TestGeneric<int, object>), _dict);

            _dict.ContainsKey(typeof(TestGeneric<,>)).IsTrue();
            _dict.ContainsKey(typeof(TestGeneric<string,int>)).IsFalse();
            _dict.ContainsKey(typeof(TestGeneric<int,object>)).IsFalse();
        }

        [TestMethod]
        public void 返り値がGenericの場合()
        {
            TypeParser.GetTypeRec(typeof(IEnumerator<>), _dict);
            _dict.Keys.Any(type => type.IsGenericParameter).IsFalse();
        }

        [TestMethod]
        public void NullableがGeneric型として登録される()
        {
            TypeParser.GetTypeRec(typeof(int?), _dict);
            _dict.ContainsKey(typeof (Nullable<>)).IsTrue();
        }

        [TestMethod]
        public void 通常のClassのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof(Object),_dict).WrappingName.Is("IGeneratedCloneForObject");
        }

        [TestMethod]
        public void GenericClassのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof (Dictionary<string, string>),_dict).WrappingName.Is("IGeneratedCloneForDictionary<T0,T1>");
        }

        [TestMethod]
        public void Arrayのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof(object[]),_dict).WrappingName.Is("IGeneratedCloneForObjectArray");
        }

        [TestMethod]
        public void Nullableのクローンインターフェース名を作成する()
        {
            new TypeWrapper(typeof(int?),_dict).WrappingName.Is("IGeneratedCloneForNullable<T0>");
        }

        [TestMethod]
        public void Genric型に実際の型を入れる()
        {
            var type = typeof(TestGeneric<string,int>);
            new TypeWrapper(type,_dict).GetWrappingNameWithType(new[] { typeof(object), typeof(double) })
                .Is("IGeneratedCloneForTestGeneric<System.Object,System.Double>");
        }


        [TestMethod]
        public void ジェネリックパラメータを型に入れた時の型付ラッパ名をとるテスト()
        {
            var type = typeof(TestGeneric<string, int>);

            var genericParamType = typeof (Task<>).GetProperty("Result").PropertyType;

            new TypeWrapper(type,_dict).GetWrappingNameWithType(new []{genericParamType})
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
            TypeParser.GetTypeRec(typeof(Testclass), _dict);

            GetWrappedTypeName(typeof(TestClassSub), _dict).Is("IGeneratedCloneForTestClassSub");
        }

        [TestMethod]
        public void 全列挙したTypeすべてでGetWrappedTypeName()
        {
            TypeParser.GetTypeRec(typeof(Form), _dict);

            foreach (var type in _dict.Keys)
            {
                GetWrappedTypeName(type, _dict);

                foreach (var pInfo in type.GetProperties().Where(prop => !((prop.GetGetMethod() != null && prop.GetGetMethod().IsStatic) || (prop.GetSetMethod() != null && prop.GetSetMethod().IsStatic))))
                {
                    
                }
            }

            var menuStripType = typeof (MenuStrip).GetMethods().Where(info => !info.IsStatic && !info.IsSpecialName ).Distinct(new ToStringComparer<MethodInfo>()).ToArray();
        }

        [TestMethod]
        public void プロパティの全列挙テスト_プロパティ3()
        {
            TypeParser.GetTypeRec(typeof(Testclass),_dict);

            var generated = _dict[typeof (Testclass)];

            generated.PropertyStrings.Count().Is(3);
            generated.PropertyStrings.ElementAt(0).Is("System.String Hoge { get; set; }");
            generated.PropertyStrings.ElementAt(1).Is("IGeneratedCloneForList<IGeneratedCloneForTestClassSub> Fuga { get; set; }");
            generated.PropertyStrings.ElementAt(2).Is("IGeneratedCloneForTestClassSub Piyo { get;  }");
        }

        [TestMethod]
        public void プロパティの全列挙テスト_プロパティ0()
        {
            var generated = new TypeWrapper(typeof(TestClassSub),_dict);

            generated.PropertyStrings.Count().Is(0);
        }

        [TestMethod]
        public void インデクサの取得テスト()
        {
            TypeParser.GetTypeRec(typeof(TestIndexer),_dict);

            _dict[typeof(TestIndexer)].PropertyStrings.ElementAt(0).Is("System.Int32 this[System.Int32 index0] { get; set; }");
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

        public List<TestClassSub> Fuga { get; set; }

        public TestClassSub Piyo { get { return null; } }
    }

    class TestClassSub
    {
        
    }

    class TestIndexer
    {
        public int this[int i]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

    class TestGeneric<T1 , T2>
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
}
