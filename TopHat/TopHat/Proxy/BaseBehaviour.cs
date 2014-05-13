using LinFu.AOP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Proxy
{
    public abstract class BaseBehaviour : IBehaviour
    {
        public virtual void OnGet<T>(Proxy<T> proxy, IInvocationInfo info)
        {
        }

        public virtual void OnSet<T>(Proxy<T> proxy, IInvocationInfo info)
        {
        }

        public virtual void OnInvoke<T>(Proxy<T> proxy, IInvocationInfo info)
        {
        }

        public virtual void OnCreate<T>(Proxy<T> proxy)
        {
        }
    }
}