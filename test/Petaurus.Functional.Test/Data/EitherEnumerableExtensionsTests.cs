using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Petaurus.Functional.Data;

using Xunit;

namespace Petaurus.Functional.Test.Data
{
    public class EitherEnumerableExtensionsTests
    {
        private static readonly Random _random = new ();
        
        [Theory]
        [MemberData(nameof(RightsData))]
        public void Rights(IEnumerable<Either<int, string>> eithers, IEnumerable<string> rights)
        {
            Assert.Equal(rights, eithers.Rights());
        }

        [Theory]
        [MemberData(nameof(LeftsData))]
        public void Lefts(IEnumerable<Either<int, string>> eithers, IEnumerable<int> lefts)
        {
            Assert.Equal(lefts, eithers.Lefts());
        }

        [Theory]
        [MemberData(nameof(PartitionEithersData))]
        public void PartitionEithers(
            IEnumerable<Either<int, string>> eithers, 
            (IReadOnlyList<int>, IReadOnlyList<string>) expected)
        {
            var actual = eithers.PartitionEithers();
            Assert.Equal(expected.Item1, actual.Lefts);
            Assert.Equal(expected.Item2, actual.Rights);
        }

        private enum Mode
        {
            Lefts,
            Rights,
            PartitionEithers
        }
        
        private static object?[] Data(Mode mode)
        {
            var list = new List<Either<int, string>>();
            var rights = new List<string>();
            var lefts = new List<int>();
            var length = _random.Next(10, 20);
            
            for (int i = 0; i < length; i++)
            {
                var value = _random.Next(100, 200);
                var isRight = _random.Next(0, 2) == 0;
                var either = isRight
                    ? Either.Right<int, string>(value.ToString())
                    : new(value);
                list.Add(either);
                if (isRight)
                {
                    rights.Add(value.ToString());
                }
                else
                {
                    lefts.Add(value);
                }
            }

            var expected = mode switch
            {
                Mode.Rights => (object) rights,
                Mode.Lefts => lefts,
                Mode.PartitionEithers => ((IReadOnlyList<int>) lefts, (IReadOnlyList<string>) rights),
                _ => throw new InvalidEnumArgumentException(nameof(mode))
            };
            return new object?[] {list, expected};
        }

        private static IEnumerable<object?[]> TestData(Mode mode) 
            => Enumerable.Range(0, 10).Select(_ => Data(mode)); 
        
        public static IEnumerable<object?[]> RightsData() => TestData(Mode.Rights);
        public static IEnumerable<object?[]> LeftsData() => TestData(Mode.Lefts);
        public static IEnumerable<object?[]> PartitionEithersData() => TestData(Mode.PartitionEithers);

    }
}