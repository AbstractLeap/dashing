using LinFu.AOP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TopHat.Proxy
{
    public interface IBehaviour
    {
        void OnGet<T>(Proxy<T> proxy, IInvocationInfo info);

        void OnSet<T>(Proxy<T> proxy, IInvocationInfo info);

        void OnInvoke<T>(Proxy<T> proxy, IInvocationInfo info);

        void OnCreate<T>(Proxy<T> proxy);
    }
}