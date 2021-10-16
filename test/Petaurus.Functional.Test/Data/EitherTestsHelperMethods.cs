using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Petaurus.Functional.Data;

using Xunit.Sdk;

namespace Petaurus.Functional.Test.Data
{
    public class EitherTestsHelperMethods
    {
        public static IEnumerable<object?[]> Pairs<T>(IEnumerable<T> list)
        {
            // Deliberate multiple enumeration
            // ReSharper disable PossibleMultipleEnumeration
            return from x in list
                from y in list
                select P(x, y);
            // ReSharper restore PossibleMultipleEnumeration
        }

        internal static TLeft GetLeft<TLeft, TRight>(Either<TLeft, TRight> either)
            => either.Match(
                x => x,
                _ => throw new XunitException($"{either} should have been a Left."));

        internal static TRight GetRight<TLeft, TRight>(Either<TLeft, TRight> either)
            => either.Match(
                _ => throw new XunitException($"{either} should have been a Right."),
                x => x);
        
        
        public static bool IsEven(int x) => x % 2 == 0;
        public static Task<bool> IsEvenAsync(int x) => Task.FromResult(x % 2 == 0);

        public static object?[] P(params object?[] parameters) => parameters;

    }
}