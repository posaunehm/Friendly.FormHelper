using System;
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
        private Dictionary<Type, string> _dict;
        private const string ClassNamePlaceHolder = "IGeneratedCloneFor";


        [TestInitialize]
        public void Setup()
        {
            _dict = new Dictionary<Type, string>();
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

            _dict.ContainsKey(typeof(int[])).IsTrue();
        }

        [TestMethod]
        public void 束縛する型が異なるジェネリック型を二つ加えても片方しか追加されない()
        {
            GetTypeRec(typeof(TestGeneric<string>), _dict);
            GetTypeRec(typeof(TestGeneric<int>), _dict);

            _dict.ContainsKey(typeof(TestGeneric<>)).IsTrue();
            _dict.ContainsKey(typeof(TestGeneric<int>)).IsFalse();
            _dict.ContainsKey(typeof(TestGeneric<string>)).IsFalse();
        }

        [TestMethod]
        public void 通常のClassのクローンインターフェース名を作成する()
        {
            var type = typeof(Object);

            MakeCloneInterfaceName(type).Is("IGeneratedCloneForObject");
        }

        [TestMethod]
        public void GenericClassのクローンインターフェース名を作成する()
        {
            var genericType = typeof (Dictionary<string, string>);

            MakeCloneInterfaceName(genericType).Is("IGeneratedCloneForDictionary<T0,T1>");
        }

        [TestMethod]
        public void Arrayのクローンインターフェース名を作成する()
        {
            var genericType = typeof(object[]);

            MakeCloneInterfaceName(genericType).Is("IGeneratedCloneForObjectArray");
        }

        private string MakeCloneInterfaceName(Type type)
        {
            var typeArguments = type.GenericTypeArguments;
            if (typeArguments.Any())
            {
                var genericParamString = Enumerable.Range(0, typeArguments.Length)
                                                   .Aggregate("", (s, i) => string.Format("{0}T{1},", s, i))
                                         .TrimEnd(',');
                var typeNameWithoutGenericParams = type.Name.Split('`')[0];

                return string.Format("{0}{1}<{2}>",ClassNamePlaceHolder, typeNameWithoutGenericParams, genericParamString);
            }
            return ClassNamePlaceHolder + type.Name.Replace("[]", "Array");
        }


        void GetTypeRec(Type type,  Dictionary<Type, string> dict)
        {
            if (type.IsValueType || type == typeof(string)) { return; }
            
            var realType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            if (_dict.ContainsKey(realType)) { return; }

            dict.Add(realType, MakeCloneInterfaceName(type));
            
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



    class Testclass
    {
        public string Hoge { get; set; }

        public List<string> Fuga { get; set; } 
    }

    class TestGeneric<T>
    {
    }
}
