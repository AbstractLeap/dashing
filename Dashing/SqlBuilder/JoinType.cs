using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dashing.SqlBuilder {
    public enum JoinType {
        InnerJoin,

        LeftJoin,

        RightJoin,

        FullOuterJoin,

        CrossJoin
    }

    public static class SqlFunctions
    {
        public static T Sum<T>(T column)
        {
            return column;
        }
    }

    public interface ISqlFunctionWriter
    {
        string WriteSql(string argsList);
    }

    public interface ISqlFunctionRegister
    {
        ISqlFunctionWriter GetWriter(MethodInfo methodInfo);
    }

    public class DefaultSqlFunctionRegister : ISqlFunctionRegister
    {
        private IDictionary<string, IList<Tuple<MethodInfo, ISqlFunctionWriter>>> register = new Dictionary<string, IList<Tuple<MethodInfo, ISqlFunctionWriter>>>();

        public void RegisterFunction(MethodInfo methodInfo, ISqlFunctionWriter writer)
        {
            if (!register.TryGetValue(methodInfo.Name, out var list))
            {
                list = new List<Tuple<MethodInfo, ISqlFunctionWriter>>();
                register.Add(methodInfo.Name, list);
            }

            list.Add(Tuple.Create(methodInfo, writer));
        }

        public ISqlFunctionWriter GetWriter(MethodInfo methodInfo)
        {
            if (register.TryGetValue(methodInfo.Name, out var list))
            {
                foreach(var tuple in list)
                {
                    if (methodInfo.Equals(tuple.Item1)) // TODO check equality, i.e. it might be that we need a custom function here
                    {
                        return tuple.Item2;
                    }
                }
            }

            return new DefaultSqlFunctionWriter(methodInfo);
        }
    }

    internal class DefaultSqlFunctionWriter : ISqlFunctionWriter
    {
        private MethodInfo methodInfo;

        public DefaultSqlFunctionWriter(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public string WriteSql(string argsList)
        {
            return $"{this.methodInfo.Name}({argsList})";
        }
    }
}