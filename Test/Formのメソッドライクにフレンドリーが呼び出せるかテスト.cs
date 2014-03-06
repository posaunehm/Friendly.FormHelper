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

        [TestMethod]
        public void メインフォームのプロパティ取得テスト()
        {
            _proc =
                Process.Start(
                    @"D:\Documents\Visual Studio 2013\Projects\Friendly.FormHelper\SampleForm\bin\Debug\SampleForm.exe");

            var wrapper = new FormWrapper(_proc);

            var sut = wrapper.MainForm;

            sut.Handle.Is(_proc.MainWindowHandle);
            sut.Text.Is("MainForm");
        }

        [TestCleanup]
        public void TearDown()
        {
            _proc.Kill();
        }
    }
}
