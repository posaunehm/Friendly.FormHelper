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

            var ret = _formAppVar[mm.MethodName](args);

            Option<object> returnValue;
            
            try
            {
                returnValue = Option.Some(ret.Core ?? NullObject);
            }
            catch (FriendlyOperationException)
            {
                returnValue = Option.None;
            }


            return returnValue.Match(
                Some: r => new ReturnMessage(
                    r == NullObject ? null : r, null, 0, mm.LogicalCallContext, (IMethodCallMessage)msg),
                None: () =>
                {
                    var friendlyProxyType = typeof (FriendlyProxy<>);
                    var constructedFriendlyProxyType = friendlyProxyType.MakeGenericType(method.ReturnType);
                    var friendlyProxy =
                        (dynamic) Activator.CreateInstance(constructedFriendlyProxyType, new object[] {ret});
                    object transparentProxy = friendlyProxy.GetTransparentProxy();

                    return new ReturnMessage(
                        transparentProxy, null, 0, mm.LogicalCallContext,
                        (IMethodCallMessage) msg);
                });
        }
    }
}