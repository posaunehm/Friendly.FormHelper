using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
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
            var args = mm.Args;

            var ret = _formAppVar[mm.MethodName](args);

            object returnValue;
            try
            {
                returnValue = ret.Core;
            }
            catch (FriendlyOperationException)
            {
                returnValue = null;
            }

            if (returnValue != null)
            {

                return new ReturnMessage(
                    returnValue, null, 0, mm.LogicalCallContext, (IMethodCallMessage)msg);
            }
            else
            {
                return new ReturnMessage(
                    new FriendlyProxy<IControlCollectionClone>(ret).GetTransparentProxy(), null, 0, mm.LogicalCallContext,
                    (IMethodCallMessage) msg);
            }
        }
    }
}