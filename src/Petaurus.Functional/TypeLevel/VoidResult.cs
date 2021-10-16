namespace Petaurus.Functional.TypeLevel;

/// <summary>
/// Convenience class for when you want to return void from something that
/// needs to return a value. A private struct declared in quite a few Microsoft
/// libraries but never published.
/// </summary>
/// <remarks>
/// Might be removed if https://github.com/dotnet/csharplang/discussions/696 
/// ever happens. Since it's structurally equivalent to 
/// <seealso cref="System.Void" /> there's no performance implications of
/// using this.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct VoidResult {

}

