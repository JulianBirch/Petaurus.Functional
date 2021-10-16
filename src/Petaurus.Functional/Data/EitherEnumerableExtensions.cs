using System.Linq;

namespace Petaurus.Functional.Data;

public static class EitherEnumerableExtensions
{
    /// <summary>
    /// Extracts from a list of Either all the Right elements. All the Right elements are extracted in order.
    /// </summary>
    /// <returns>A lazily evaluated list.</returns>
    /// <remarks>Uses LINQ internally so that it can take part in LINQ fusion.</remarks>
    public static IEnumerable<TRight> Rights<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        return eithers.SelectMany(x => x.RightAsEnumerable());
    }

    /// <summary>
    /// Extracts from a list of Either all the Left elements. All the Left elements are extracted in order.
    /// </summary>
    /// <returns>A lazily evaluated list.</returns>
    /// <remarks>Uses LINQ internally so that it can take part in LINQ fusion.</remarks>
    public static IEnumerable<TLeft> Lefts<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        return eithers.SelectMany(x => x.LeftAsEnumerable());
    }

    /// <summary>
    /// Extracts all the Right and Left elements into a tuple. Note that unlike <seealso cref="Rights" />
    /// and <seealso cref="Lefts" /> this is eagerly evaluated. 
    /// </summary>
    public static (IReadOnlyList<TLeft> Lefts, IReadOnlyList<TRight> Rights) PartitionEithers<TLeft, TRight>(
            this IEnumerable<Either<TLeft, TRight>> eithers)
    {
        var count = eithers.TryGetNonEnumeratedCount(out var c) ? c : 10;
        var lefts = new List<TLeft>(count);
        var rights = new List<TRight>(count);
        foreach (var either in eithers)
        {
            if (either.TryExtractInternal(out var left, out var right))
            {
                rights.Add(right);
            }
            else
            {
                lefts.Add(left);
            }
        }
        if (lefts.Count < count / 3)
        {
            lefts.TrimExcess();
        }
        if (rights.Count < count / 3)
        {
            rights.TrimExcess();
        }
        return new(lefts, rights);
    }
}