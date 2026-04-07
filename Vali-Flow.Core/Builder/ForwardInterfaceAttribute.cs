namespace Vali_Flow.Core.Builder;

/// <summary>
/// Marker attribute indicating that the decorated field participates in interface forwarding.
/// Used by the Vali-Flow.Core.Generator Roslyn source generator to emit one-line delegation
/// wrappers in <see cref="ValiFlow{T}"/> and <see cref="ValiFlowQuery{T}"/>.
/// </summary>
/// <remarks>
/// Until the source generator is shipped as part of the package, forwarding is implemented
/// manually in the builder classes. This attribute serves as documentation of intent and
/// will activate the generator in a future release.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class ForwardInterfaceAttribute : Attribute { }
