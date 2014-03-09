using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class Fクローンインターフェース作成テスト
    {
        [TestMethod]
        public void Formのクローンインターフェースを作成する()
        {
            var formType = typeof (Form);

            string output = "";

            foreach (var props in formType.GetProperties())
            {
                output += string.Format("{0} {1} {2}{3}",props.PropertyType.FullName,props.Name,"{get;set;};",Environment.NewLine);
            }

            

        }
    }

    class testclass
    {
        public string Hoge { get; set; }
    }


}
