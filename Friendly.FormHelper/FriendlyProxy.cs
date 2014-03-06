using System.Reflection;
using System.Runtime.Remoting.Messaging;
using Codeer.Friendly;
using Codeer.Friendly.Windows;

namespace Friendly.FormHelper
{
    class FriendlyProxy<TInterface> : System.Runtime.Remoting.Proxies.RealProxy
    {
        private readonly AppVar _formAppVar;

        public FriendlyProxy(AppVar formAppVar)
            : base(typeof(TInterface))
        {
            _formAppVar = formAppVar;
        }


        public override IMessage Invoke(IMessage msg)
        {
            var mm = msg as IMethodMessage;

            var method = (MethodInfo)mm.MethodBase;
            object[] args = mm.Args;


            //string callSite = string.Format("{0}.{1}", method.DeclaringType.FullName, mm.MethodName);
            var ret = _formAppVar[mm.MethodName]();

            

            return new ReturnMessage(
                ret.Core, null, 0, mm.LogicalCallContext, (IMethodCallMessage)msg);
        }
    }
}