using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using Codeer.Friendly;
using Codeer.Friendly.Windows;
using LangExt;

namespace Friendly.FormHelper
{
    class FriendlyProxy<TInterface> : System.Runtime.Remoting.Proxies.RealProxy
    {
        private readonly AppVar _formAppVar;

        private static readonly object NullObject = new object();

        public FriendlyProxy(AppVar formAppVar)
            : base(typeof(TInterface))
        {
            _formAppVar = formAppVar;
        }


        public override IMessage Invoke(IMessage msg)
        {
            var mm = msg as IMethodMessage;

            var method = (MethodInfo)mm.MethodBase;
            var args = mm.Args;

            var returnedAppVal = _formAppVar[mm.MethodName](args);


            var returnValue = TryGetCoreValue(returnedAppVal);


            return returnValue.Match(
                Some: r => new ReturnMessage(
                    r == NullObject ? null : r, null, 0, mm.LogicalCallContext, (IMethodCallMessage)msg),
                None: () => new ReturnMessage(
                    WrapChildAppVar(method, returnedAppVal), null, 0, mm.LogicalCallContext,
                    (IMethodCallMessage) msg));
        }

        private static Option<object> TryGetCoreValue(AppVar ret)
        {
            Option<object> returnValue;
            try
            {
                returnValue = Option.Some(ret.Core ?? NullObject);
            }
            catch (FriendlyOperationException)
            {
                returnValue = Option.None;
            }
            return returnValue;
        }

        private static object WrapChildAppVar(MethodInfo method, AppVar ret)
        {
            var friendlyProxyType = typeof (FriendlyProxy<>).MakeGenericType(method.ReturnType);
            dynamic friendlyProxy = Activator.CreateInstance(friendlyProxyType, new object[] {ret});
            return friendlyProxy.GetTransparentProxy();
        }
    }
}