// Required to use 'record' types when targeting netstandard2.0.
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Polyfill that enables C# <c>record</c> types (and <c>init</c>-only setters) when
    /// targeting frameworks that do not include this type in their BCL (e.g. netstandard2.0).
    /// The compiler emits a reference to this class for every <c>init</c> accessor; providing
    /// an internal stub here satisfies that reference without introducing a real dependency.
    /// </summary>
    internal static class IsExternalInit { }
}
