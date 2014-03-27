using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Friendly.FormHelper;

namespace Test
{
    [TestClass]
    public class Formのメソッドライクにフレンドリーが呼び出せるかテスト
    {
        private Process _proc;

        [TestInitialize]
        public void Setup()
        {
            _proc =
                Process.Start(
                    @"..\..\..\SampleForm\bin\Debug\SampleForm.exe");
        }

        [TestMethod]
        public void メインフォームの確定値プロパティ取得テスト()
        {
            var wrapper = new FormWrapper(_proc);

            var sut = wrapper.MainForm;

            sut.Handle.Is(_proc.MainWindowHandle);
            sut.Text.Is("MainForm");
        }

        [TestMethod]
        public void メインフォームの非確定値プロパティ取得テスト()
        {
            var wrapper = new FormWrapper(_proc);

            var sut = wrapper.MainForm;

            sut.Controls.Count.Is(1);
            sut.Controls[0].Text.Is("hoge");
            sut.Controls["button1"].Text.Is("hoge");
            sut.MdiChildren.Length.Is(0);
        }


        [TestMethod]
        public void メインフォームの非確定値プロパティ取得テスト_その2()
        {
            var wrapper = new FormWrapper(_proc);

            var sut = wrapper.MainForm;

            sut.ParentForm.IsNull();
        }

        [TestCleanup]
        public void TearDown()
        {
            _proc.Kill();
        }
    }
}
