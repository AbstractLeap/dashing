using System;
using System.Linq.Expressions;

namespace TopHat
{
    public interface IWhere<T> : IWhereExecute<T>, IOrder<T>
    {
    }
}