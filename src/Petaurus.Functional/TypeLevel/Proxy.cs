namespace Petaurus.Functional.TypeLevel;

/// <summary>
/// Useful for when you're in a situation where you need to specify some
/// type parameters, but not others. With this you can supply an extra
/// parameter that takes a <see cref="Proxy{T}" /> and it will then fully deduce
/// the correct type parameters.
/// </summary>
/// <remarks>Since it's structurally equivalent to 
/// <seealso cref="System.Void" /> there's no performance implications of
/// using this.</remarks>
[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Proxy<T>
{
    public override bool Equals(object? obj) => obj is Proxy<T>;

    public override int GetHashCode() => typeof(T).GetHashCode();
}
