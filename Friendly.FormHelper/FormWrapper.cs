using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;

namespace Friendly.FormHelper
{
    public class FormWrapper : IDisposable
    {
        private Process _proc;
        private WindowsAppFriend _app;

        public FormWrapper(Process proc)
        {
            _proc = proc;
            _app = new WindowsAppFriend(_proc, "v4.0.30319");
        }

        public Form MainForm
        {
            get
            {
                var proxy = new FriendlyProxy<Form>(_app["System.Windows.Forms.Control.FromHandle"](_proc.MainWindowHandle)).GetTransparentProxy() as Form;

                return proxy;
            }
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _app.Dispose();
            }
        }

        ~FormWrapper()
        {
            this.Dispose(false);
        }

    }
}