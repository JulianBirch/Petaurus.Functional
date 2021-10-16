using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Petaurus.Functional.Data;
using Petaurus.Functional.Linq;

using Xunit;

namespace Petaurus.Functional.Test.Linq
{
    public class EnumerableExtensionsTests
    {
        private static readonly Random _random = new ();

        private static Either<string, int> TryParse(string s)
        {
            return int.TryParse(s, out var v)
                ? new(v)
                : new("Fail " + s);
        }

        private static Task<Either<string, int>> TryParseAsync(string s) => Task.FromResult(TryParse(s));

        [Theory]
        [MemberData(nameof(TraverseData))]
        public void Traverse(IEnumerable<string> input, Either<string, IReadOnlyList<int>> expected)
        {
            AssertEquivalent(expected, input.Traverse(TryParse));
        }

        [Theory]
        [MemberData(nameof(TraverseData))]
        public async Task TraverseAsyncAwait(IEnumerable<string> input, Either<string, IReadOnlyList<int>> expected)
        {
            AssertEquivalent(expected, await input.TraverseAsyncAwait(TryParseAsync));
        }
        
        [Theory]
        [MemberData(nameof(TraverseDataAsync))]
        public async Task TraverseAsync(IAsyncEnumerable<string> input, Either<string, IReadOnlyList<int>> expected)
        {
            AssertEquivalent(expected, await input.TraverseAsync(TryParse));
        }
        
        [Theory]
        [MemberData(nameof(TraverseDataAsync))]
        public async Task TraverseAsyncAwait2(IAsyncEnumerable<string> input, Either<string, IReadOnlyList<int>> expected)
        {
            AssertEquivalent(expected, await input.TraverseAsyncAwait(TryParseAsync));
        }

        [Theory]
        [MemberData(nameof(TraverseDictionaryData))]
        public void TraverseDictionary(
            IReadOnlyDictionary<Guid, string> input, 
            Either<string, IReadOnlyDictionary<Guid, int>> expected)
        {
            AssertEquivalent(expected, input.Traverse(TryParse));
        }
        
        [Theory]
        [MemberData(nameof(TraverseDictionaryData))]
        public async Task TraverseDictionaryAsync(
            IReadOnlyDictionary<Guid, string> input, 
            Either<string, IReadOnlyDictionary<Guid, int>> expected)
        {
            AssertEquivalent(expected, await input.TraverseAsync(TryParseAsync));
        }

        private static void AssertEquivalent<T>(Either<string, T> expected, Either<string, T> actual)
        where T : class
        {
            if (expected.TryExtract(out var expectedLeft, out var expectedRight))
            {
                var actualRight = actual.RightOrDefault();
                Assert.Equal(expectedRight, actualRight);
            }
            else
            {
                var actualLeft = actual.LeftOrDefault("Error");
                Assert.Equal(expectedLeft, actualLeft);
            }
        }

        public static IEnumerable<object?[]> TraverseData()
        {
            return Enumerable.Range(0, 10)
                .SelectMany(_ => InternalTraverseData())
                .Select(x => new object?[] {x.Item1, x.Item2});
        }
        
        public static IEnumerable<object?[]> TraverseDictionaryData()
        {
            return Enumerable.Range(0, 10)
                .SelectMany(_ => InternalTraverseDictionaryData())
                .Select(x => new object?[] {x.Item1, x.Item2});
        }
            
        public static IEnumerable<object?[]> TraverseDataAsync()
        {
            return Enumerable.Range(0, 10)
                .SelectMany(_ => InternalTraverseData())
                .Select(x => new object?[] {ToAsyncEnumerable(x.Item1), x.Item2});
        }

        private static async IAsyncEnumerable<string> ToAsyncEnumerable(IEnumerable<string> list)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            foreach (var item in list)
            {
                yield return item;
            }
        }
        
        public static IEnumerable<(IEnumerable<string>, Either<string, IReadOnlyList<int>>)> InternalTraverseData()
        {
            var length = _random.Next(10, 15);
            var rights = Enumerable.Range(0, length)
                .Select(_ => _random.Next(0, 100))
                .ToList();
            var input = rights.Select(x => x.ToString()).ToList();
            var failInput = input.ToList();
            var failIndex = _random.Next(0, length);
            var failContent = Guid.NewGuid().ToString();
            failInput.Insert(failIndex, failContent);
            yield return (input, Either.Right<string, IReadOnlyList<int>>(rights));
            yield return (failInput, Either.Left<string, IReadOnlyList<int>>("Fail " + failContent));
        }
        
        public static IEnumerable<(IReadOnlyDictionary<Guid, string>, Either<string, IReadOnlyDictionary<Guid, int>>)> 
            InternalTraverseDictionaryData()
        {
            var length = _random.Next(10, 15);
            var input = new Dictionary<Guid, string>();
            var rights = new Dictionary<Guid, int>();
            for (var index = 0; index < length; index++)
            {
                var value = _random.Next(0, 100);
                var key = Guid.NewGuid();
                rights.Add(key, value);
                input.Add(key, value.ToString());
            }
            var failInput = input.ToDictionary(kv => kv.Key, kv => kv.Value);
            var failContent = Guid.NewGuid().ToString();
            failInput.Add(Guid.NewGuid(), failContent);
            yield return (input, Either.Right<string, IReadOnlyDictionary<Guid, int>>(rights));
            yield return (failInput, Either.Left<string, IReadOnlyDictionary<Guid, int>>("Fail " + failContent));
        }
    }
}