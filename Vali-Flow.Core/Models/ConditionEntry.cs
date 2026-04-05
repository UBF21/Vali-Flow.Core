using System.Linq.Expressions;

namespace Vali_Flow.Core.Models;

internal sealed record ConditionEntry<T>(
    Expression<Func<T, bool>> Condition,
    bool IsAnd,
    string? ErrorCode,
    string? Message,
    Func<string>? MessageFactory,
    string? PropertyPath,
    Func<T, bool>? CompiledFunc,
    Severity Severity);
