using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class クローンインターフェース作成テスト
    {
        private Dictionary<Type, string> _dict= new Dictionary<Type, string>();

        [TestMethod]
        public void Formのクローンインターフェースを作成する()
        {
            var formType = typeof (Form);

            GetTypeRec(formType,_dict);
        }


        [TestMethod]
        public void test()
        {
            var testStr = "IGeneratedCloneForIEnumerable`1";

            testStr.Split('`')[0].Is("IGeneratedCloneForIEnumerable");
        }

        void GetTypeRec(Type type,  Dictionary<Type, string> dict)
        {
            if (type.IsValueType || type == typeof(string) || type.IsArray) { return; }
            
            if(_dict.ContainsKey(type)){return;}

            dict.Add(type, type.Name);
            
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



    class testclass
    {
        public string Hoge { get; set; }
    }


}
