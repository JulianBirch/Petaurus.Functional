using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Petaurus.Functional.Data;
using Petaurus.Functional.TypeLevel;
using Petaurus.Functional.Test.Data.TestConcepts;

using Xunit;

namespace Petaurus.Functional.Test.Data
{
    public class EitherTests
    {
        [Theory]
        [ClassData(typeof(ExpectationSource))]
        internal async Task MapRightAsync<TRightInitial, TRightExpected>(
            Expectation<TRightInitial, TRightExpected> right)
        {
            var initial = Either.Right<VoidResult, TRightInitial>(right.Initial);
            var expected = Either.Right<VoidResult, TRightExpected>(right.Expected);
            var actual = await initial.MapRightAsync(right.Operation.InvokeAsync);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [ClassData(typeof(ExpectationSource))]
        internal Task MapRight<TRightInitial, TRightExpected>(
            Expectation<TRightInitial, TRightExpected> right)
        {
            var initial = Either.Right<VoidResult, TRightInitial>(right.Initial);
            var expected = Either.Right<VoidResult, TRightExpected>(right.Expected);
            var actual = initial.MapRight(right.Operation.Invoke);

            Assert.Equal(expected, actual);
            return Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(IgnoreExpectationData))]
        public Task MapRightIgnore<TRightInitial, TRightExpected, TValue>(
            Operation<TRightInitial, TRightExpected> operation, ValueHolder<TValue> value)
        {
            var initial = value.Left<TRightInitial>();
            var actual = initial.MapRight(operation.Invoke);

            Assert.Equal(value.Value, EitherTestsHelperMethods.GetLeft(actual));
            return Task.CompletedTask;
        }
        
        [Theory]
        [MemberData(nameof(IgnoreExpectationData))]
        public async Task MapRightAsyncIgnore<TRightInitial, TRightExpected, TValue>(
            Operation<TRightInitial, TRightExpected> operation, ValueHolder<TValue> value)
        {
            var initial = value.Left<TRightInitial>();
            var actual = await initial.MapRightAsync(operation.InvokeAsync);

            Assert.Equal(value.Value, EitherTestsHelperMethods.GetLeft(actual));
        }
        
        [Theory]
        [ClassData(typeof(ExpectationSource))]
        internal async Task MapLeftAsync<TLeftInitial, TLeftExpected>(
            Expectation<TLeftInitial, TLeftExpected> left)
        {
            var initial = Either.Left<TLeftInitial, VoidResult>(left.Initial);
            var expected = Either.Left<TLeftExpected, VoidResult>(left.Expected);
            var actual = await initial.MapLeftAsync(left.Operation.InvokeAsync);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [ClassData(typeof(ExpectationSource))]
        internal Task MapLeft<TLeftInitial, TLeftExpected>(
            Expectation<TLeftInitial, TLeftExpected> left)
        {
            var initial = Either.Left<TLeftInitial, VoidResult>(left.Initial);
            var expected = Either.Left<TLeftExpected, VoidResult>(left.Expected);
            var actual = initial.MapLeft(left.Operation.Invoke);

            Assert.Equal(expected, actual);
            return Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(IgnoreExpectationData))]
        public Task MapLeftIgnore<TLeftInitial, TLeftExpected, TValue>(
            Operation<TLeftInitial, TLeftExpected> operation, ValueHolder<TValue> value)
        {
            var initial = value.Right<TLeftInitial>();
            var actual = initial.MapLeft(operation.Invoke);

            Assert.Equal(value.Value, EitherTestsHelperMethods.GetRight(actual));
            return Task.CompletedTask;
        }
        
        [Theory]
        [MemberData(nameof(IgnoreExpectationData))]
        public async Task MapLeftAsyncIgnore<TLeftInitial, TLeftExpected, TValue>(
            Operation<TLeftInitial, TLeftExpected> operation, ValueHolder<TValue> value)
        {
            var initial = value.Right<TLeftInitial>();
            var actual = await initial.MapLeftAsync(operation.InvokeAsync);

            Assert.Equal(value.Value, EitherTestsHelperMethods.GetRight(actual));
        }
        
        [Theory]
        [MemberData(nameof(FlipData))]
        public void Flip<T1, T2>(ValueHolder<T1> value, Proxy<T2> ignored, bool isLeft)
        {
            if (isLeft)
            {
                var either = value.Left(ignored);
                Assert.Equal(value.Value, EitherTestsHelperMethods.GetRight(either.Flip()));
            }
            else
            {
                var either = value.Right(ignored);
                Assert.Equal(value.Value, EitherTestsHelperMethods.GetLeft(either.Flip()));
            }
        }
        
        [Theory]
        [MemberData(nameof(ExpectationPairs))]
        internal Task MapBoth<TLeftInitial, TLeftExpected, TRightInitial, TRightExpected>(
            Expectation<TLeftInitial, TLeftExpected> left, Expectation<TRightInitial, TRightExpected> right, bool isRight)
        {
            
            Either<TLeftInitial, TRightInitial> initial = isRight
                ? new(right.Initial)
                : new(left.Initial);
            Either<TLeftExpected, TRightExpected> expected = isRight
                ? new(right.Expected)
                : new(left.Expected);
            var actual = initial.MapBoth(left.Operation.Invoke, right.Operation.Invoke);

            Assert.Equal(expected, actual);
            return Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(ExpectationPairs))]
        internal async Task MapBothAsync<TLeftInitial, TLeftExpected, TRightInitial, TRightExpected>(
            Expectation<TLeftInitial, TLeftExpected> left, 
            Expectation<TRightInitial, TRightExpected> right, 
            bool isRight)
        {
            Either<TLeftInitial, TRightInitial> initial = isRight
                ? new(right.Initial)
                : new(left.Initial);
            Either<TLeftExpected, TRightExpected> expected = isRight
                ? new(right.Expected)
                : new(left.Expected);
            var actual = await initial.MapBothAsync(left.Operation.InvokeAsync, right.Operation.InvokeAsync);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(FlipData))]
        public void TestToString<T1, T2>(ValueHolder<T1> value, Proxy<T2> ignored, bool isLeft)
        {
            if (isLeft)
            {
                var either = value.Left(ignored);
                Assert.Equal("Left:" + (value.Value?.ToString() ?? "null"), either.ToString());
            }
            else
            {
                var either = value.Right(ignored);
                Assert.Equal("Right:" + (value.Value?.ToString() ?? "null"), either.ToString());
            }
        }
        
        [Theory]
        [MemberData(nameof(EqualsData))]
        public void TestEquals<TLeft1, TRight1, TLeft2, TRight2>(
            Either<TLeft1, TRight1> value1, 
            Either<TLeft2, TRight2> value2,
            bool shouldBeEqual)
        {
            if (shouldBeEqual)
            {
                Assert.Equal(value1, (object) value2);
            }
            else
            {
                Assert.NotEqual(value1, (object) value2);
            }
        }

        [Theory]
        [MemberData(nameof(EithersData))]
        public void IsLeft<TLeft, TRight>(Either<TLeft, TRight> either, bool isRight)
        {
            Assert.Equal(!isRight, either.IsLeft);
        }
        
        [Theory]
        [MemberData(nameof(EithersData))]
        public void IsRight<TLeft, TRight>(Either<TLeft, TRight> either, bool isRight)
        {
            Assert.Equal(isRight, either.IsRight);
        }
        
        [Theory]
        [MemberData(nameof(ValueHolderPairs))]
        public void LeftOrDefault<T>(ValueHolder<T> value, ValueHolder<T> defaultValue)
        {
            var left = value.Left<VoidResult>();
            var right = Either.Right<T, VoidResult>(default);
            Assert.Equal(value.Value, left.LeftOrDefault());
            Assert.Equal(value.Value, left.LeftOrDefault(defaultValue.Value));
            Assert.Equal(default, right.LeftOrDefault());
            Assert.Equal(defaultValue.Value, right.LeftOrDefault(defaultValue.Value));
        }

        [Theory]
        [MemberData(nameof(ValueHolderPairs))]
        public void RightOrDefault<T>(ValueHolder<T> value, ValueHolder<T> defaultValue)
        {
            var right = value.Right<VoidResult>();
            var left = Either.Left<VoidResult, T>(default);
            Assert.Equal(default, left.RightOrDefault());
            Assert.Equal(defaultValue.Value, left.RightOrDefault(defaultValue.Value));
            Assert.Equal(value.Value, right.RightOrDefault());
            Assert.Equal(value.Value, right.RightOrDefault(defaultValue.Value));
        }

        [Theory]
        [MemberData(nameof(ValueHolders))]
        public void LeftAsEnumerable<T>(ValueHolder<T> value)
        {
            var right = value.Right<VoidResult>();
            var left = value.Left<VoidResult>();
            Assert.Empty(right.LeftAsEnumerable());
            var extracted = Assert.Single(left.LeftAsEnumerable());
            Assert.Equal(value.Value, extracted);
        }
        
        [Theory]
        [MemberData(nameof(ValueHolders))]
        public void RightAsEnumerable<T>(ValueHolder<T> value)
        {
            var right = value.Right<VoidResult>();
            var left = value.Left<VoidResult>();
            Assert.Empty(left.RightAsEnumerable());
            var extracted = Assert.Single(right.RightAsEnumerable());
            Assert.Equal(value.Value, extracted);
        }
        
        [Theory]
        [MemberData(nameof(WithData))]
        public void WithRight<TLeft, TRight, T>(Either<TLeft, TRight> either, ValueHolder<T> value)
        {
            var result = either.WithRight(value.Value);
            Assert.Equal(typeof(TLeft), result.GetType().GenericTypeArguments[0]);
            Assert.Equal(value.Value, result.RightOrDefault());
        }

        [Theory]
        [MemberData(nameof(WithData))]
        public void WithLeft<TLeft, TRight, T>(Either<TLeft, TRight> either, ValueHolder<T> value)
        {
            var result = either.WithLeft(value.Value);
            Assert.Equal(typeof(TRight), result.GetType().GenericTypeArguments[1]);
            Assert.Equal(value.Value, result.LeftOrDefault());
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        [InlineData(15)]
        public void Match(int i)
        {
            // For such an important function, my test cases are kind of rubbish
            var left = Either.Left<int, VoidResult>(i);
            var right = Either.Right<int, VoidResult>(default);
            var isEven = EitherTestsHelperMethods.IsEven(i);
            Assert.Equal(isEven, left.Match(EitherTestsHelperMethods.IsEven, _ => false));
            Assert.False(right.Match(EitherTestsHelperMethods.IsEven, _ => false));
        }

        [Theory]
        [InlineData(null, "2", 2)]
        [InlineData(null, "3", 3)]
        [InlineData(null, "X", "Parse Failure")]
        [InlineData(null, "Y", "Parse Failure")]
        [InlineData("NoValue", null, "NoValue")]
        [InlineData("NoValue2", null, "NoValue2")]
        public void BindRight(string? left, string? right, object result)
        {
            Either<string, int> TryParse(string s)
                => int.TryParse(s, out var v) ? new(v) : new("Parse Failure");

            var initial =
                left is not null
                    ? Either.Left<string, string>(left!)
                    : Either.Right<string, string>(right!);

            Either<string, int> expected = result is int intResult
                ? new(intResult)
                : new((string)result);

            var actual = initial.BindRight(TryParse);
            
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, "2", 2)]
        [InlineData(null, "3", 3)]
        [InlineData(null, "X", "Parse Failure")]
        [InlineData(null, "Y", "Parse Failure")]
        [InlineData("NoValue", null, "NoValue")]
        [InlineData("NoValue2", null, "NoValue2")]
        public async Task BindRightAsync(string? left, string? right, object result)
        {
            Task<Either<string, int>> TryParseAsync(string s)
                => Task.FromResult(int.TryParse(s, out var v) 
                    ? Either.Right<string, int>(v) 
                    : new("Parse Failure"));

            var initial =
                left is not null
                    ? Either.Left<string, string>(left!)
                    : Either.Right<string, string>(right!);

            Either<string, int> expected = result is int intResult
                ? new(intResult)
                : new((string)result);

            var actual = await initial.BindRightAsync(TryParseAsync);
            
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task MatchAsync(int i)
        {
            var left = Either.Left<int, VoidResult>(i);
            var right = Either.Right<int, VoidResult>(default);
            var isEven = EitherTestsHelperMethods.IsEven(i);
            Assert.Equal(isEven, await left.MatchAsync(EitherTestsHelperMethods.IsEvenAsync, _ => Task.FromResult(false)));
            Assert.False(await right.MatchAsync(EitherTestsHelperMethods.IsEvenAsync, _ => Task.FromResult(false)));
        }

        [Theory]
        [MemberData(nameof(CombineData))]
        public static void Combine<TLeft, TRight>(Either<TLeft, TRight> x, Either<TLeft, TRight> y)
        {
            var actual = x.Combine(y);
            var expected = x.IsRight ? x : y;
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object?[]> EithersData()
        {
            return from value1 in ExpectationSource.Values
                from proxy1 in ExpectationSource.OtherTypes
                from isRight1 in new[] {false, true}
                let either1 = value1.ToEither(proxy1, isRight1)
                select EitherTestsHelperMethods.P(either1, isRight1);    
        }
        
        public static IEnumerable<object?[]> ValueHolderPairs()
        {
            return EitherTestsHelperMethods.Pairs(ExpectationSource.ValueHoldersInt32)
                .Concat(EitherTestsHelperMethods.Pairs(ExpectationSource.ValueHoldersDouble))
                .Concat(EitherTestsHelperMethods.Pairs(ExpectationSource.ValueHoldersString));
        }

        private static IEnumerable<Either<TLeft, TRight>> Eithers<TLeft, TRight>(
            IEnumerable<ValueHolder<TLeft>> lefts, 
            IEnumerable<ValueHolder<TRight>> rights)
        {
            return lefts.Select(l => l.Left<TRight>())
                .Concat(rights.Select(r => r.Right<TLeft>()));
        }

        public static IEnumerable<object?[]> CombineData()
        {
            var eitherSets = new[]
            {
                (IEnumerable<object>)Eithers(ExpectationSource.ValueHoldersInt32,
                    ExpectationSource.ValueHoldersInt32),
                Eithers(ExpectationSource.ValueHoldersInt32, ExpectationSource.ValueHoldersDouble),
                Eithers(ExpectationSource.ValueHoldersInt32, ExpectationSource.ValueHoldersString),
                Eithers(ExpectationSource.ValueHoldersDouble, ExpectationSource.ValueHoldersInt32),
                Eithers(ExpectationSource.ValueHoldersDouble, ExpectationSource.ValueHoldersDouble),
                Eithers(ExpectationSource.ValueHoldersDouble, ExpectationSource.ValueHoldersString),
                Eithers(ExpectationSource.ValueHoldersString, ExpectationSource.ValueHoldersInt32),
                Eithers(ExpectationSource.ValueHoldersString, ExpectationSource.ValueHoldersDouble),
                Eithers(ExpectationSource.ValueHoldersString, ExpectationSource.ValueHoldersString),
            };
            return eitherSets.SelectMany(EitherTestsHelperMethods.Pairs);
        }
        
        public static IEnumerable<object?[]> ValueHolders()
        {
            return ExpectationSource.Values.Select(x => EitherTestsHelperMethods.P(x));
        }

        public static IEnumerable<object?[]> WithData()
        {
            return from value1 in ExpectationSource.Values
                from proxy1 in ExpectationSource.OtherTypes
                from isRight1 in new[] {false, true}
                from value2 in ExpectationSource.Values
                let either1 = value1.ToEither(proxy1, isRight1)
                select EitherTestsHelperMethods.P(either1, value2);
        }

        public static IEnumerable<object?[]> EqualsData()
        {
            return from value1 in ExpectationSource.Values
                from value2 in ExpectationSource.Values
                from proxy1 in ExpectationSource.OtherTypes
                from proxy2 in ExpectationSource.OtherTypes
                from isRight1 in new[] {false, true}
                from isRight2 in new[] {false, true}
                let either1 = value1.ToEither(proxy1, isRight1)
                let either2 = value2.ToEither(proxy2, isRight2)
                let shouldBeEqual = isRight1 == isRight2 && value1.Equals(value2) && proxy1.Equals(proxy2)
                select EitherTestsHelperMethods.P(either1, either2, shouldBeEqual);
        }

        public static IEnumerable<object?[]> IgnoreExpectationData()
        {
            return from expectation in Operation.AllOperations()
                from preserved in ExpectationSource.Values
                select EitherTestsHelperMethods.P(expectation, preserved);
        }
        
        public static IEnumerable<object?[]> FlipData()
        {
            return from value in ExpectationSource.Values
                from otherType in ExpectationSource.OtherTypes
                from isRight in new[] {false, true}
                select EitherTestsHelperMethods.P(value, otherType, isRight);
        }

        public static IEnumerable<object?[]> ExpectationPairs()
        {
            return from left in ExpectationSource.AllExpectations()
                from right in ExpectationSource.AllExpectations()
                from isRight in new[] {false, true}
                select EitherTestsHelperMethods.P(left, right, isRight);
        }
    }
}