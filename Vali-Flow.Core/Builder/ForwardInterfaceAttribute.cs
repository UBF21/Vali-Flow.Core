namespace Vali_Flow.Core.Builder;

/// <summary>
/// Marks a private field in a partial builder class so that
/// <c>Vali-Flow.Core.Generator</c> emits a public forwarding method
/// for every method on the field's interface type.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class ForwardInterfaceAttribute : Attribute { }
