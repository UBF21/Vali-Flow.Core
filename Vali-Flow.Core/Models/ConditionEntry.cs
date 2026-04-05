using System.Linq.Expressions;

namespace Vali_Flow.Core.Models;

internal sealed record ConditionEntry<T>
{
    public Expression<Func<T, bool>> Condition { get; init; }
    public bool IsAnd { get; init; }
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }
    public Func<string>? MessageFactory { get; init; }
    public string? PropertyPath { get; init; }
    public Severity Severity { get; init; }

    // Lazy compilation — thread-safe by Lazy<T> default (LazyThreadSafetyMode.ExecutionAndPublication)
    public Lazy<Func<T, bool>> CompiledFunc { get; init; }

    public ConditionEntry(
        Expression<Func<T, bool>> condition,
        bool isAnd,
        string? errorCode,
        string? message,
        Func<string>? messageFactory,
        string? propertyPath,
        Severity severity)
    {
        Condition = condition;
        IsAnd = isAnd;
        ErrorCode = errorCode;
        Message = message;
        MessageFactory = messageFactory;
        PropertyPath = propertyPath;
        Severity = severity;
        CompiledFunc = new Lazy<Func<T, bool>>(() => condition.Compile());
    }
}
