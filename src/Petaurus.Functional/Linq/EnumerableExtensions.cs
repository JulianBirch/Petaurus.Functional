using System.Linq;
using System.Threading;

using Petaurus.Functional.Data;

namespace Petaurus.Functional.Linq;

/// <summary>
/// Mostly just multiple implementations of Traverse, which is like Select but supports Either
/// short-circuiting.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Uses a projection that returns an <seealso cref="Either<TLeft, TRight>" />. 
    /// Returns the first Left value found or all of the rights.
    /// </summary>
    /// <returns></returns>
    public static Either<TLeft, IReadOnlyList<TRight>> Traverse<TInput, TLeft, TRight>(
        this IEnumerable<TInput> list, 
        Func<TInput, Either<TLeft, TRight>> projection,
        int? estimatedCount = null)
    {
        var count = list.TryGetNonEnumeratedCount(out var c) ? c : estimatedCount ?? 10;
        var rights = new List<TRight>(count);
        foreach (var either in list) {
            if (projection(either).TryExtractInternal(out var left, out var right)) {
                rights.Add(right);
            } else {
                return new(left);
            }
        }
        return new(rights);
    }

    /// <summary>
    /// Uses a projection that returns an <seealso cref="Either<TLeft, TRight>" />. 
    /// Returns the first Left value found or all of the rights.
    /// </summary>
    /// <returns></returns>
    public static async Task<Either<TLeft, IReadOnlyList<TRight>>> TraverseAsyncAwait<TInput, TLeft, TRight>(
            this IEnumerable<TInput> list, 
            Func<TInput, Task<Either<TLeft, TRight>>> projection, 
            int? estimatedCount = null) {
        var count = list.TryGetNonEnumeratedCount(out var c) ? c : estimatedCount ?? 10;
        return await TraverseAsyncAwait(list.AsAsyncEnumerable(), projection, count);
    }

    /// <summary>
    /// Uses a projection that returns an <seealso cref="Either<TLeft, TRight>" />. 
    /// Returns the first Left value found or all of the rights.
    /// </summary>
    /// <returns></returns>
    public static async Task<Either<TLeft, IReadOnlyList<TRight>>> TraverseAsync<TInput, TLeft, TRight>(
            this IAsyncEnumerable<TInput> list,
            Func<TInput, Either<TLeft, TRight>> projection,
            int estimatedCount = 10)
        => await TraverseAsyncAwait(list, x => Task.FromResult(projection(x)), estimatedCount);

   /// <summary>
    /// Uses a projection that returns an <seealso cref="Either<TLeft, TRight>" />. 
    /// Returns the first Left value found or all of the rights.
    /// </summary>
    /// <returns></returns>
    public static async Task<Either<TLeft, IReadOnlyList<TRight>>> TraverseAsyncAwait<TInput, TLeft, TRight>(
        this IAsyncEnumerable<TInput> list, 
        Func<TInput, Task<Either<TLeft, TRight>>> projection, 
        int estimatedCount = 10)
    {
        var rights = new List<TRight>(estimatedCount);
        await foreach (var either in list) {
            var (isRight, left, right) = await projection(either);
            if (isRight) {
                rights.Add(right);
            } else {
                return new(left);
            }
        }
        return new(rights);
    }

    // This function is in System.Linq.Async but I'm trying to avoid taking a dependency
    private static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> list) => new AsyncEnumerable<T>(list);

    internal record AsyncEnumerable<T>(IEnumerable<T> Enumerable) : IAsyncEnumerable<T> {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token)
            => new AsyncEnumerator<T>(Enumerable.GetEnumerator());
    }

    internal record AsyncEnumerator<T>(IEnumerator<T> Enumerator) : IAsyncEnumerator<T> {
        public ValueTask<bool> MoveNextAsync() => new (Enumerator.MoveNext());

        public T Current => Enumerator.Current;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    /// <summary>
    /// Uses a projection that returns an <seealso cref="Either<TLeft, TRight>" />. 
    /// Returns the first Left value found or all of the rights.
    /// </summary>
    public static Either<TLeft, IReadOnlyDictionary<TKey, TRight>> Traverse<TInput, TKey, TLeft, TRight>(
            this IReadOnlyDictionary<TKey, TInput> dictionary, 
            Func<TInput, Either<TLeft, TRight>> projection)
        where TKey : notnull
    {
        var comparer = (dictionary as Dictionary<TKey, TInput>)?.Comparer ?? EqualityComparer<TKey>.Default;
        var rights = new Dictionary<TKey, TRight>(dictionary.Count, comparer);
        foreach (var (key, either) in dictionary) {
            var (isRight, left, right) = projection(either);
            if (isRight) {
                rights.Add(key, right);
            } else {
                return new(left);
            }
        }
        return new(rights);
    }

    /// <summary>
    /// Uses a projection that returns an <seealso cref="Either<TLeft, TRight>" />. 
    /// Returns the first Left value found or a mapped dictionary with all of the elements.
    /// </summary>
    public static async Task<Either<TLeft, IReadOnlyDictionary<TKey, TRight>>> TraverseAsync<TInput, TKey, TLeft, TRight>(
            this IReadOnlyDictionary<TKey, TInput> dictionary, 
            Func<TInput, Task<Either<TLeft, TRight>>> projection)
        where TKey : notnull
    {
        var comparer = (dictionary as Dictionary<TKey, TInput>)?.Comparer ?? EqualityComparer<TKey>.Default;
        var rights = new Dictionary<TKey, TRight>(dictionary.Count, comparer);
        foreach (var (key, either) in dictionary) {
            var (isRight, left, right) = await projection(either);
            if (isRight) {
               rights.Add(key, right);
            } else {
                return new(left);
            }
        }
        return new(rights);
    }
}