namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines lifecycle management operations for an expression builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type.</typeparam>
public interface IExpressionLifecycle<out TBuilder>
{
    /// <summary>
    /// Creates a new, independent builder pre-populated with all conditions from this instance.
    /// The clone starts unfrozen so new conditions can be appended freely.
    /// </summary>
    /// <returns>A deep copy of the current builder with the same accumulated conditions.</returns>
    TBuilder Clone();

    /// <summary>
    /// Permanently seals this builder as a fork point. Subsequent mutation calls
    /// return a new independent clone rather than modifying this instance.
    /// </summary>
    /// <remarks>Use this when you want to share a base configuration across multiple builders without risk of mutation. Call <see cref="Clone"/> to obtain a mutable copy after freezing.</remarks>
    void Freeze();
}
