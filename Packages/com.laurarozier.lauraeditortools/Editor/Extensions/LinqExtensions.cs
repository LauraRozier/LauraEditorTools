#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace LauraEditor.Tools.Editor.Extensions {
    public static class LinqExtensions
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> listA, IEnumerable<T> listB, Func<T, T, bool> lambda) =>
            listA.Except(listB, new LambdaComparer<T>(lambda));
    }

    internal class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _expression;

        public LambdaComparer(Func<T, T, bool> lambda) => _expression = lambda;

        public bool Equals(T x, T y) => _expression(x, y);

        /*
            If you just return 0 for the hash the Equals comparer will kick in.
            The underlying evaluation checks the hash and then short circuits the evaluation if it is false.
            Otherwise, it checks the Equals. If you force the hash to be true (by assuming 0 for both objects),
            you will always fall through to the Equals check which is what we are always going for.
        */
        public int GetHashCode(T obj) => 0;
    }
}
#endif
