using Petaurus.Functional.Data;
using Petaurus.Functional.TypeLevel;

using System.Collections.Generic;
using System.Linq;

using Petaurus.Functional.Test.Data.TestConcepts;

namespace Petaurus.Functional.Test.Data
{
    public class ExpectationSource : IEnumerable<object?[]>
    {
        public IEnumerator<object?[]> GetEnumerator()
        {
            return AllExpectations().Select(x => new object?[] { x }).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public static IEnumerable<IExpectation> Create<TInitial, TExpected>(Operation<TInitial, TExpected> operation,
            params TInitial[] initialValues)
            => initialValues.Select(i =>
                new Expectation<TInitial, TExpected>(i, operation.InvokeForceSync(i), operation));
        
        public static IEnumerable<IExpectation> AllExpectations() =>
            Create(Operation.AddOneIfEvenNullIfOdd, 3, 4, 5, 7, 100, 201)
                .Concat(Create(Operation.DoubleToString, 1.0, 1.5, 2.300, 5))
                .Concat(Create(Operation.Reverse, "Hello", "World", null));

        private static IEnumerable<ValueHolder<T>> ValueHolders<T>(params T[] parameters) 
            => parameters.Select(p => new ValueHolder<T>(p));

        public static readonly IEnumerable<ValueHolder<int>> ValueHoldersInt32 = ValueHolders(3, 5);
        public static readonly IEnumerable<ValueHolder<double>> ValueHoldersDouble = ValueHolders(1.0, 9.0); 
        public static readonly IEnumerable<ValueHolder<string?>> ValueHoldersString = ValueHolders((string?) "Hello", "World", null);

        public static IEnumerable<object> Eithers() =>
            from v in Values
            from p in OtherTypes
            from isRight in new [] {false, true} 
            select v.ToEither(p, isRight);
        
        public static IEnumerable<IValueHolder> Values =>
            ValueHolders(3, 5)
                .Cast<IValueHolder>()
                .Concat(ValueHolders(1.0, 9.0))
                .Concat(ValueHolders((string?) "Hello", "World", null));    

        public static IEnumerable<object> OtherTypes =
            new object[] {default(Proxy<int>), default(Proxy<double?>), default(Proxy<string>)};
    }
}