using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Vali_Flow.Core.Generator;

/// <summary>
/// Incremental source generator that reads private fields marked with
/// <c>[ForwardInterface]</c> in a partial class and emits a sibling
/// partial file with one public (or explicit) forwarding method per
/// interface member, traversing the full interface inheritance hierarchy.
/// </summary>
[Generator]
public sealed class ForwardingGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "Vali_Flow.Core.Builder.ForwardInterfaceAttribute";

    private static readonly SymbolDisplayFormat FullyQualifiedNullable =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    // ── Pure-data models ──────────────────────────────────────────────────────

    /// <param name="SignatureKey">Name|TypeArity|param-types — used to detect conflicts.</param>
    /// <param name="SourceIfaceFqn">
    ///   Fully-qualified instantiated interface name (e.g. IComparableExpression&lt;ValiFlow&lt;T&gt;,T&gt;).
    ///   Used as explicit-implementation prefix when a conflict is detected.
    /// </param>
    private sealed record MethodEntry(
        string ReturnType,
        string MethodName,
        string TypeParams,
        string Parameters,
        string Constraints,
        string CallTypeArgs,
        string CallArgs,
        string SignatureKey,
        string SourceIfaceFqn);

    private sealed record FieldEntry(
        string ClassName,
        string Namespace,
        string TypeParamList,
        string FieldName,
        ImmutableArray<MethodEntry> Methods);

    private sealed record ClassModel(
        string ClassName,
        string Namespace,
        string TypeParamList,
        ImmutableArray<FieldEntry> Fields);

    // ── Pipeline ──────────────────────────────────────────────────────────────

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: static (node, _) => node is VariableDeclaratorSyntax,
                transform: static (ctx, ct) => ExtractEntry(ctx, ct))
            .Where(static e => e is not null)
            .Collect()
            .Select(static (items, _) => GroupByClass(items!));

        context.RegisterSourceOutput(pipeline, static (spc, classModels) =>
        {
            foreach (var model in classModels)
                spc.AddSource($"{model.ClassName}.Forwarding.g.cs", Emit(model));
        });
    }

    // ── Extraction ────────────────────────────────────────────────────────────

    private static FieldEntry? ExtractEntry(
        GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetSymbol is not IFieldSymbol field) return null;
        if (field.Type is not INamedTypeSymbol ifaceType) return null;
        if (ifaceType.TypeKind != TypeKind.Interface) return null;

        var containingType = field.ContainingType;
        if (containingType is null) return null;

        var ns = containingType.ContainingNamespace?.ToDisplayString() ?? "";
        var typeParamList = containingType.TypeParameters.Length > 0
            ? "<" + string.Join(", ", containingType.TypeParameters.Select(tp => tp.Name)) + ">"
            : "";

        var methods = CollectAllMethods(ifaceType, ct);
        if (methods.IsEmpty) return null;

        return new FieldEntry(
            containingType.Name, ns, typeParamList,
            field.Name, methods);
    }

    /// <summary>
    /// Collects methods from the field's direct interface AND all inherited interfaces.
    /// </summary>
    private static ImmutableArray<MethodEntry> CollectAllMethods(
        INamedTypeSymbol ifaceType, CancellationToken ct)
    {
        var result = ImmutableArray.CreateBuilder<MethodEntry>();

        // Direct members + all inherited interface members
        var allInterfaces = new List<INamedTypeSymbol> { ifaceType };
        allInterfaces.AddRange(ifaceType.AllInterfaces);

        foreach (var iface in allInterfaces)
        {
            ct.ThrowIfCancellationRequested();
            foreach (var member in iface.GetMembers())
            {
                if (member is not IMethodSymbol method) continue;
                if (method.MethodKind != MethodKind.Ordinary || method.IsStatic) continue;

                var sourceFqn = iface.ToDisplayString(FullyQualifiedNullable);
                result.Add(BuildMethodEntry(method, sourceFqn));
            }
        }

        return result.ToImmutable();
    }

    private static MethodEntry BuildMethodEntry(IMethodSymbol method, string sourceIfaceFqn)
    {
        var returnType = method.ReturnType.ToDisplayString(FullyQualifiedNullable);
        var typeParams = method.TypeParameters.Length > 0
            ? "<" + string.Join(", ", method.TypeParameters.Select(tp => tp.Name)) + ">"
            : "";
        var parameters = string.Join(", ", method.Parameters.Select(RenderParameter));
        var constraintParts = method.TypeParameters
            .Select(RenderConstraint)
            .Where(c => c.Length > 0)
            .ToArray();
        var constraints = constraintParts.Length > 0
            ? string.Join(" ", constraintParts)
            : "";
        var callTypeArgs = method.TypeParameters.Length > 0
            ? "<" + string.Join(", ", method.TypeParameters.Select(tp => tp.Name)) + ">"
            : "";
        var callArgs = string.Join(", ", method.Parameters.Select(p => p.Name));

        // Signature key for conflict detection: name + type-arity + param-type-list
        var paramTypes = string.Join(",",
            method.Parameters.Select(p =>
                p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        var sigKey = $"{method.Name}|{method.TypeParameters.Length}|{paramTypes}";

        return new MethodEntry(returnType, method.Name, typeParams, parameters,
                               constraints, callTypeArgs, callArgs,
                               sigKey, sourceIfaceFqn);
    }

    // ── Parameter rendering ───────────────────────────────────────────────────

    private static string RenderParameter(IParameterSymbol p)
    {
        var typeStr = p.Type.ToDisplayString(FullyQualifiedNullable);
        return p.HasExplicitDefaultValue
            ? $"{typeStr} {p.Name}{RenderDefault(p)}"
            : $"{typeStr} {p.Name}";
    }

    private static string RenderDefault(IParameterSymbol p)
    {
        var value = p.ExplicitDefaultValue;
        if (value is null) return " = null";

        var type = p.Type;
        if (type.TypeKind == TypeKind.Enum)
        {
            foreach (var member in type.GetMembers().OfType<IFieldSymbol>())
            {
                if (member.HasConstantValue && Equals(member.ConstantValue, value))
                {
                    var fqType = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    return $" = {fqType}.{member.Name}";
                }
            }
            var fqFallback = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return $" = ({fqFallback})({value})";
        }

        return value switch
        {
            string s => $" = \"{s}\"",
            bool b   => b ? " = true" : " = false",
            char c   => $" = '{c}'",
            _        => $" = {value}"
        };
    }

    // ── Constraint rendering ──────────────────────────────────────────────────

    private static string RenderConstraint(ITypeParameterSymbol tp)
    {
        var parts = new List<string>();
        if (tp.HasReferenceTypeConstraint)  parts.Add("class");
        if (tp.HasValueTypeConstraint)      parts.Add("struct");
        if (tp.HasNotNullConstraint)        parts.Add("notnull");
        if (tp.HasUnmanagedTypeConstraint)  parts.Add("unmanaged");
        foreach (var ct in tp.ConstraintTypes)
            parts.Add(ct.ToDisplayString(FullyQualifiedNullable));
        if (tp.HasConstructorConstraint)    parts.Add("new()");
        return parts.Count == 0 ? "" : $"where {tp.Name} : {string.Join(", ", parts)}";
    }

    // ── Grouping ──────────────────────────────────────────────────────────────

    private static ImmutableArray<ClassModel> GroupByClass(
        ImmutableArray<FieldEntry> entries)
    {
        var byClass = entries
            .GroupBy(e => (e.Namespace, e.ClassName, e.TypeParamList));

        var result = ImmutableArray.CreateBuilder<ClassModel>();
        foreach (var group in byClass)
        {
            result.Add(new ClassModel(
                group.Key.ClassName,
                group.Key.Namespace,
                group.Key.TypeParamList,
                group.ToImmutableArray()));
        }
        return result.ToImmutable();
    }

    // ── Emission ──────────────────────────────────────────────────────────────

    private static string Emit(ClassModel model)
    {
        // Build a global signature map across all fields to detect conflicts.
        // A conflict = same (name, type-arity, param-types) from two different source interfaces.
        // Key: SignatureKey  Value: the source-interface-fqn that "wins" (is emitted as public)
        var firstOccurrence = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var field in model.Fields)
        {
            foreach (var m in field.Methods)
            {
                if (!firstOccurrence.ContainsKey(m.SignatureKey))
                    firstOccurrence[m.SignatureKey] = m.SourceIfaceFqn;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member");
        sb.AppendLine();
        sb.AppendLine($"namespace {model.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"public partial class {model.ClassName}{model.TypeParamList}");
        sb.AppendLine("{");

        // Track which (field, sigKey) pairs have already been emitted to avoid duplicates
        // (the same method can appear twice via the direct interface and its inherited version).
        var emitted = new HashSet<string>(StringComparer.Ordinal);

        foreach (var field in model.Fields)
        {
            foreach (var m in field.Methods)
            {
                // De-duplicate: each (sigKey, sourceIfaceFqn) is emitted at most once per field
                var dedupKey = $"{field.FieldName}|{m.SignatureKey}|{m.SourceIfaceFqn}";
                if (!emitted.Add(dedupKey)) continue;

                bool isPublic = string.Equals(
                    firstOccurrence[m.SignatureKey], m.SourceIfaceFqn,
                    StringComparison.Ordinal);

                if (isPublic)
                {
                    // public ValiFlow<T> MethodName<TKey>(...) where TKey : ... => _field.MethodName<TKey>(...);
                    sb.Append($"    public {m.ReturnType} {m.MethodName}{m.TypeParams}({m.Parameters})");
                    if (m.Constraints.Length > 0)
                        sb.Append($" {m.Constraints}");
                    sb.AppendLine($" => {field.FieldName}.{m.MethodName}{m.CallTypeArgs}({m.CallArgs});");
                }
                else
                {
                    // ValiFlow<T> IComparableExpression<ValiFlow<T>,T>.MethodName<TKey>(...) => _field.MethodName<TKey>(...);
                    // Note: explicit implementations do NOT repeat the constraint.
                    sb.Append($"    {m.ReturnType} {m.SourceIfaceFqn}.{m.MethodName}{m.TypeParams}({m.Parameters})");
                    sb.AppendLine($" => {field.FieldName}.{m.MethodName}{m.CallTypeArgs}({m.CallArgs});");
                }

                sb.AppendLine();
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
