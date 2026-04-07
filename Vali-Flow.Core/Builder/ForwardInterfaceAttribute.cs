namespace Vali_Flow.Core.Builder;

/// <summary>
/// Marker attribute indicating that the decorated field participates in interface forwarding.
/// The <c>Vali-Flow.Core.Generator</c> Roslyn incremental source generator reads this attribute
/// and emits a sibling <c>*.Forwarding.g.cs</c> partial file with one public delegation method
/// per interface member (traversing the full interface inheritance hierarchy).
/// </summary>
/// <remarks>
/// The generator is active: it is referenced via <c>OutputItemType="Analyzer"</c> in the
/// library's <c>.csproj</c>. Any field decorated with this attribute in a <c>partial</c> class
/// will have its forwarding methods generated at build time — no manual wrappers needed.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class ForwardInterfaceAttribute : Attribute { }
