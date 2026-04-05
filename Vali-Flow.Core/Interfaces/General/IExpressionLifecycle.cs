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
    TBuilder Clone();

    /// <summary>
    /// Permanently seals this builder as a fork point. Subsequent mutation calls
    /// return a new independent clone rather than modifying this instance.
    /// </summary>
    void Freeze();
}
