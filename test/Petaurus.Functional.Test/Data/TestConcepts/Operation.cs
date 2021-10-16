using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Petaurus.Functional.Test.Data.TestConcepts;

namespace Petaurus.Functional.Test
{
    public static class Operation
    {
        public static Operation<TInitial, TExpected> CreateSync<TInitial, TExpected>(
            Func<TInitial, TExpected> projection,
            string description) => new(description, i => Task.FromResult(projection(i)), projection);

        public static Operation<int, int?> AddOneIfEvenNullIfOdd = CreateSync(
            (int x) => x % 2 == 0 ? x + 1 : default(int?),
            "Add One If Even, Null If Odd");

        public static Operation<double, string> DoubleToString = CreateSync(
            (double x) => x.ToString(),
            "Double To String");

        public static Operation<string?, string?> Reverse = CreateSync(
            (string? x) => ReverseString(x),
            "Reverse String And Handle Nulls");

        private static string? ReverseString(string? s)
        {
            if (s is null)
            {
                return null;
            }

            var array = s.ToCharArray();
            Array.Reverse(array);
            return new(array);
        }

        public static IEnumerable<IOperation> AllOperations() => new IOperation[] { AddOneIfEvenNullIfOdd, DoubleToString, Reverse};
    }

    public class Operation<TInitial, TExpected> : IOperation
    {
        // Sure would be useful to have Either to use here, wouldn't it?
        private Func<TInitial, TExpected>? _syncProjection = null;
        private Func<TInitial, Task<TExpected>> _asyncProjection;

        internal Operation(
            string description,
            Func<TInitial, Task<TExpected>> asyncProjection,
            Func<TInitial, TExpected>? syncProjection = null)
        {
            _asyncProjection = asyncProjection;
            _syncProjection = syncProjection;
            Description = description;
        }

        public bool IsAsyncOnly => _syncProjection is null;
        public string Description { get; init; }

        public Type InitialType => typeof(TInitial);
        public Type ExpectedType => typeof(TExpected);

        public override string ToString() => Description;

        public TExpected Invoke(TInitial initial) => _syncProjection!(initial);

        public async Task<TExpected> InvokeAsync(TInitial initial) => await _asyncProjection(initial);

        // Yeah, I just did sync over async... it's horrible.
        public TExpected InvokeForceSync(TInitial initial) =>
            _syncProjection is null
                ? InvokeAsync(initial).GetAwaiter().GetResult()
                : _syncProjection(initial);
    }
}