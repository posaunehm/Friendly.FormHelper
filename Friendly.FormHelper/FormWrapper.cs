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
using InterfaceGenerator;

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

        public IGeneratedCloneForForm MainForm
        {
            get
            {
                var proxy = new FriendlyProxy<IGeneratedCloneForForm>(_app["System.Windows.Forms.Control.FromHandle"](_proc.MainWindowHandle)).GetTransparentProxy() as IGeneratedCloneForForm;

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

    public interface IFormClone
    {
        IntPtr Handle { get;  }
        string Text { get; set; }
        IControlCollectionClone Controls { get;  }
        IFormClone ParentForm { get; set; }
    }

    public interface IControlCollectionClone
    {
        int Count { get;  }
        IControlClone this[int i] { get; }
    }

    public interface IControlClone
    {
        string Text { get; set; }
    }
}