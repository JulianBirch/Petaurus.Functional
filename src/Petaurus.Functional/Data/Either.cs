using Petaurus.Functional.TypeLevel;

namespace Petaurus.Functional.Data;

public static class Either {
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft left, Proxy<TRight> right = default) {
        return new(left);
    } 

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight right, Proxy<TLeft> left = default) {
        return new(right);
    }

    /// <summary>
    /// Gets the contained value.
    /// </summary>
    /// <returns>True if the contained value was a Right.</returns>
    /// <remarks>This is the answer to the age-old question "How do you get values out of an Either"?</remarks>
    public static bool TryExtract<TLeft, TRight>(this Either<TLeft, TRight> either, [MaybeNullWhen(true)] out TLeft left, [MaybeNullWhen(false)] out TRight right)
    where TLeft : class
    where TRight : class
        => either.TryExtractInternal(out left, out right);

    /// <summary>
    /// Gets the left struct value or a null.
    /// </summary>
    public static TLeft? LeftAsNullable<TLeft, TRight>(this Either<TLeft, TRight> either) 
    where TLeft : struct
    {
        return either.TryExtractInternal(out var result, out var _) ? default : result;
    }

    public static TRight? RightAsNullable<TLeft, TRight>(this Either<TLeft, TRight> either) 
    where TRight : struct
    {
        return either.TryExtractInternal(out var _, out var result) ? result : default;
    }

    /// <summary>
    /// Extracts the value from a symmetric either (i.e. Either{T, T}) whether it's in a left
    /// or a right.
    /// </summary>
    /// <remarks>Surprisingly, this isn't in Haskell's base.</remarks>
    public static T GetValue<T>(this Either<T, T> either) {
        return either.TryExtractInternal(out var left, out var right) ? right : left;
    }

    /// <summary>
    /// Provides a custom equality comparer on the basis of component equality comparers.
    /// </summary>
    public static IEqualityComparer<Either<TLeft, TRight>> EqualityComparer<TLeft, TRight>(
            IEqualityComparer<TLeft>? leftComparer, 
            IEqualityComparer<TRight>? rightComparer) {
        leftComparer ??= EqualityComparer<TLeft>.Default;
        rightComparer ??= EqualityComparer<TRight>.Default;
        return new EitherEqualityComparer<TLeft, TRight>(leftComparer, rightComparer);
    }

    /// <summary>
    /// Provides a custom comparer on the basis of component comparers.
    /// </summary>
    public static IComparer<Either<TLeft, TRight>> Comparer<TLeft, TRight>(
            IComparer<TLeft>? leftComparer, 
            IComparer<TRight>? rightComparer) {
        leftComparer ??= Comparer<TLeft>.Default;
        rightComparer ??= Comparer<TRight>.Default;
        return new EitherComparer<TLeft, TRight>(leftComparer, rightComparer);
    }

    private record EitherEqualityComparer<TLeft, TRight>(
            IEqualityComparer<TLeft> LeftComparer, 
            IEqualityComparer<TRight> RightComparer) 
        : IEqualityComparer<Either<TLeft, TRight>> {
                
        public bool Equals(Either<TLeft, TRight>? x, Either<TLeft, TRight>? y) {
            if (x is null) {
                return y is null;
            }
            if (y is null) {
                return false;
            }
            if (x.TryExtractInternal(out var leftX, out var rightX)) {
                return y.TryExtractInternal(out _, out var rightY) && RightComparer.Equals(rightX, rightY);
            }
            return !y.TryExtractInternal(out var leftY, out _) && LeftComparer.Equals(leftX, leftY);
        }

        public int GetHashCode(Either<TLeft, TRight> x) {
            const int LeftNullHashCode = 23;
            const int RightNullHashCode = 37;

            if (x.TryExtractInternal(out var left, out var right)) {
                return right is null ? RightNullHashCode : RightComparer.GetHashCode(right);
            }

            return left is null ? LeftNullHashCode : LeftComparer.GetHashCode(left);
        }
    }

    private record EitherComparer<TLeft, TRight>(
            IComparer<TLeft> LeftComparer,
            IComparer<TRight> RightComparer)
        : IComparer<Either<TLeft, TRight>>
    {
        public int Compare(Either<TLeft, TRight>? x, Either<TLeft, TRight>? y) {
            if (x is null) {
                return y is null ? 0 : 1;
            }
            if (y is null) {
                return -1;
            }
            var isRightComparison = x.IsRight.CompareTo(y.IsRight);
            if (isRightComparison != 0) {
                return isRightComparison;
            }
            var (isRight, leftX, rightX) = x;
            var (_, leftY, rightY) = y;
            return isRight
                ? RightComparer.Compare(rightX, rightY)
                : LeftComparer.Compare(leftX, leftY);
        }
    }
}

/// <summary>
/// <para>The Either type represents values with two possibilities: a
/// value of type <see cref="Either{TLeft, TRight}" /> is either
/// a "left <typeparam cref="TLeft" />" or a 
/// "right <typeparam cref="TRight"/>". The Either type is sometimes used to 
/// represent a value which is either correct or an error; 
/// by convention, <typeparam cref="TLeft" />" is used to hold an error value 
/// and <typeparam cref="Right" />" is used to hold a correct value.
/// (mnemonic: "right" also means "correct").
/// </para>
/// </summary>
/// <remarks>
/// <para>Note that TLeft and TRight can be the same type. We refer to this as a "symmetric either".<para>
/// <para>Every method with a "Left" in the name should have a corresponding method with a "Right" in the name,
/// and vice versa. Equally, any method that takes functions should have synchronous and asynchronous versions.</para> 
/// <para>For those coming from a Haskell perspective, Foldable is addressed through the 
/// <seealso cref="IEnumerable<TRight>" /> implementation, "bifold" is called <seealso cref="MapBoth" />,
/// "fmap" is <seealso cref="MapRight" />
/// </remark>
[DebuggerDisplay("{ToString()}")]
public sealed class Either<TLeft, TRight> : IEquatable<Either<TLeft, TRight>> {
    private readonly TLeft _left;
    private readonly TRight _right;

    private static readonly IEqualityComparer<Either<TLeft, TRight>> DefaultEqualityComparer = Either.EqualityComparer<TLeft, TRight>(null, null);

    public Either(TLeft left) : this(false, left, default!) 
    {
    }

    public Either(TRight right) : this(true, default!, right) 
    {
    }

    private Either(bool isRight, TLeft left, TRight right) {
        IsRight = isRight;
        _left = left;
        _right = right;
    }

    /// <summary>
    /// Creates an <seealso cref='Either{TRight, TLeft}' /> with the right and left swapped around.
    /// </summary>
    public Either<TRight, TLeft> Flip() => new (!IsRight, _right, _left);
    
    /// <inheritdoc />
    public override string ToString() {
        if (IsRight) {
            if (_right is null) {
                return "Right:null";
            }
            return "Right:" + _right;
        }
        if (_left is null) {
            return "Left:null";
        }
        return "Left:" + _left;
    }

    /// <inheritdoc />
    public override int GetHashCode() => DefaultEqualityComparer.GetHashCode(this);

    public bool Equals(Either<TLeft, TRight>? other)
    {
        return other is not null && DefaultEqualityComparer.Equals(this, other);
    }

    /// <inheritdoc />
    public override bool Equals(object? other)
    {
        return other is Either<TLeft, TRight> either && DefaultEqualityComparer.Equals(this, either);
    }
    
    internal bool TryExtractInternal([MaybeNullWhen(true)] out TLeft left, [MaybeNullWhen(false)] out TRight right) {
        left = _left;
        right = _right;
        return IsRight;
    }

    internal void Deconstruct(out bool isRight, out TLeft left, out TRight right) {
        isRight = IsRight;
        left = _left;
        right = _right;
    }

    /// <summary>
    /// Gets whether or not the value is a <typeparam cref="TLeft"/>.
    /// </summary>
    public bool IsLeft => !IsRight;

    /// <summary>
    /// Gets whether or not the value is a <typeparam cref="TRight"/>.
    /// </summary>
    public bool IsRight { get; }

    /// <summary>
    /// Gets the left value or default(TLeft) if the Either is a Right. If the <typeparam cref="TLeft" /> is a struct, 
    /// consider using <seealso cref="Either.LeftAsNullable{TLeft, TRight}(Either{TLeft, TRight})" /> instead. 
    /// </summary>
    /// <returns>The contained value or default!.</returns>
    [return: MaybeNull]
    public TLeft LeftOrDefault() => IsRight ? default! : _left;

    /// <summary>
    /// Gets the right value or default if the Either is a Left. If the <typeparam cref="TRight" /> is a struct, 
    /// consider using <seealso cref="Either.RightAsNullable{TLeft, TRight}(Either{TLeft, TRight})" /> instead.
    /// </summary>
    /// <returns>The contained value or default!.</returns>
    /// <remarks>Note that the nullability of the return type differs between the two overloads.</remarks>
    [return: MaybeNull]
    public TRight RightOrDefault() => IsRight ? _right : default!;

    /// <summary>
    /// Gets the left value or <paramref cref="defaultValue" /> if the Either is a Right. 
    /// </summary>
    /// <returns>The contained value or default!.</returns>
    /// <remarks>Note that the nullability of the return type differs between the two overloads.</remarks>
    public TLeft LeftOrDefault(TLeft defaultValue) => IsRight ? defaultValue : _left;

    /// <summary>
    /// Gets the right value or <paramref cref="defaultValue" /> if the Either is a Left.
    /// </summary>
    /// <returns>The contained value or default!.</returns>
    /// <remarks>Note that the nullability of the return type differs between the two overloads.</remarks>
    public TRight RightOrDefault(TRight defaultValue) => IsRight ? _right : defaultValue; 


    /// <summary>
    /// Returns an <seealso cref="IEnumerable<TLeft>" /> that returns the contained value if it is a Left,
    /// or no values if it is a Right.
    /// </summary>
    public IEnumerable<TLeft> LeftAsEnumerable() 
    {
        if (!IsRight) {
            yield return _left;
        } 
    }

    /// <summary>
    /// Returns an <seealso cref="IEnumerable<TRight>" /> that returns the contained value if it is a Right,
    /// or no values if it is a Left.
    /// </summary>
    public IEnumerable<TRight> RightAsEnumerable() 
    {
        if (IsRight) {
            yield return _right;
        } 
    }

    /// <summary>
    /// Produces a new Either with the supplied "Right" value.
    /// </summary>
    /// <remarks>Mostly useful as it preserves the <typeparam cref="TLeft" /> type information.</remarks>
    public Either<TLeft, TRightOut> WithRight<TRightOut>(TRightOut value) => new(value);

    /// <summary>
    /// Produces a new Either with the supplied "Left" value.
    /// </summary>
    /// <remarks>Mostly useful as it preserves the <typeparam cref="TRight" /> type information.</remarks>
    public Either<TLeftOut, TRight> WithLeft<TLeftOut>(TLeftOut value) => new(value);

    // Methods after this point get a bit hard-core functional. It's okay to not bother using these
    // because they're pretty much all convenience methods for what's gone on before.

    /// <summary>
    /// Case analysis, functional programming style. Allows you to map an either to a <typeparam cref="TResult" />
    /// by passing in a function for mapping the left and the right possibilities.
    /// </summary>
    public TResult Match<TResult>(Func<TLeft, TResult> leftProjection, Func<TRight, TResult> rightProjection) 
        => IsRight ? rightProjection(_right) : leftProjection(_left);

    /// <summary>
    /// Case analysis, functional programming style. Allows you to map an either to a <typeparam cref="TResult" />
    /// by passing in a function for mapping the left and the right possibilities.
    /// </summary>
    public Task<TResult> MatchAsync<TResult>(Func<TLeft, Task<TResult>> leftProjection, Func<TRight, Task<TResult>> rightProjection) 
        => IsRight ? rightProjection(_right) : leftProjection(_left);


    /// <summary>
    /// Maps the right value if the either is a "Right", or preserves the "Left" value.
    /// </summary>
    public Either<TLeft, TRightOut> MapRight<TRightOut>(Func<TRight, TRightOut> projection)
        => IsRight ? new(projection(_right)) : new(_left);

    /// <summary>
    /// Maps the left value if the either is a "Left", or preserves the "Right" value.
    /// </summary>
    public Either<TLeftOut, TRight> MapLeft<TLeftOut>(Func<TLeft, TLeftOut> projection)
        => IsRight ? new(_right) : new(projection(_left));

    /// <summary>
    /// Maps the left value if the either is a "Left", or the right value if it's a "Right".
    /// </summary>
    public Either<TLeftOut, TRightOut> MapBoth<TLeftOut, TRightOut>(
        Func<TLeft, TLeftOut> leftProjection, 
        Func<TRight, TRightOut> rightProjection)
        => IsRight ? new(rightProjection(_right)) : new(leftProjection(_left));

    /// <summary>
    /// Maps the left value if the either is a "Left", or preserves the "Right" value.
    /// </summary>
    public async Task<Either<TLeftOut, TRightOut>> MapBothAsync<TLeftOut, TRightOut>(
        Func<TLeft, Task<TLeftOut>> leftProjection, 
        Func<TRight, Task<TRightOut>> rightProjection)
        => IsRight ? new(await rightProjection(_right)) : new(await leftProjection(_left));

    /// <summary>
    /// Uses the result of the <paramref cref="projection" /> if the either is a "Right".
    /// Otherwise preserves the "Left".
    /// </summary>
    /// <remarks>
    /// Note that this operation isn't symmetric and we don't offer the reversed operation.
    /// </remarks>
    public Either<TLeft, TRightOut> BindRight<TRightOut>(Func<TRight, Either<TLeft, TRightOut>> projection)
        => IsRight ? projection(_right) : new(_left);
    
    /// <summary>
    /// Uses the result of the <paramref cref="projection" /> if the either is a "Right".
    /// Otherwise preserves the "Left".
    /// </summary>
    /// <remarks>
    /// Note that this operation isn't symmetric and we don't offer the reversed operation.
    /// </remarks>
    public async Task<Either<TLeft, TRightOut>> BindRightAsync<TRightOut>(Func<TRight, Task<Either<TLeft, TRightOut>>> projection)
        => IsRight ? await projection(_right) : new(_left);

    /// <summary>
    /// Maps the right value if the either is a "Right", or preserves the "Left" value.
    /// </summary>
    public async Task<Either<TLeft, TRightOut>> MapRightAsync<TRightOut>(Func<TRight, Task<TRightOut>> projection)
        => IsRight ? new(await projection(_right)) : new(_left);

    /// <summary>
    /// Maps the left value if the either is a "Left", or preserves the "Right" value.
    /// </summary>
    public async Task<Either<TLeftOut, TRight>> MapLeftAsync<TLeftOut>(Func<TLeft, Task<TLeftOut>> projection)
        => IsRight ? new(_right) : new(await projection(_left));

    /// <summary>
    /// Takes the first "Right" value, or the last value if none are "Right".
    /// </summary>
    public Either<TLeft, TRight> Combine(Either<TLeft, TRight> other) => IsRight ? this : other;
}