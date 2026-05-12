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
    /// <summary>Fully-qualified name of the attribute that marks fields for forwarding.</summary>
    private const string AttributeFullName = "Vali_Flow.Core.Builder.ForwardInterfaceAttribute";

    /// <summary>
    /// <see cref="SymbolDisplayFormat"/> based on <see cref="SymbolDisplayFormat.FullyQualifiedFormat"/>
    /// with nullable reference type modifiers included (e.g. <c>string?</c>).
    /// </summary>
    private static readonly SymbolDisplayFormat FullyQualifiedNullable =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    // ── Pure-data models ──────────────────────────────────────────────────────

    /// <summary>Immutable data model representing a single interface method to be forwarded.</summary>
    /// <param name="ReturnType">Fully-qualified return type string, including nullable annotations.</param>
    /// <param name="MethodName">Simple name of the method.</param>
    /// <param name="TypeParams">Generic type-parameter list (e.g. <c>&lt;TKey&gt;</c>) or empty string.</param>
    /// <param name="Parameters">Rendered parameter list (types + names + defaults) ready for source emission.</param>
    /// <param name="Constraints">Rendered <c>where</c> constraints for generic type parameters, or empty string.</param>
    /// <param name="CallTypeArgs">Type arguments to use at the call site (e.g. <c>&lt;TKey&gt;</c>) or empty string.</param>
    /// <param name="CallArgs">Comma-separated argument names to pass at the call site.</param>
    /// <param name="SignatureKey">Conflict-detection key in the form <c>Name|TypeArity|param-types</c>.</param>
    /// <param name="SourceIfaceFqn">
    ///   Fully-qualified instantiated interface name (e.g. <c>IComparableExpression&lt;ValiFlow&lt;T&gt;,T&gt;</c>).
    ///   Used as the explicit-implementation prefix when a signature conflict is detected.
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

    /// <summary>Immutable data model representing a single <c>[ForwardInterface]</c>-annotated field.</summary>
    /// <param name="ClassName">Simple name of the containing partial class.</param>
    /// <param name="Namespace">Namespace of the containing partial class.</param>
    /// <param name="TypeParamList">Generic type-parameter list of the containing class (e.g. <c>&lt;T&gt;</c>) or empty string.</param>
    /// <param name="FieldName">Name of the field whose interface members are to be forwarded.</param>
    /// <param name="Methods">All <see cref="MethodEntry"/> records collected from the field's interface hierarchy.</param>
    private sealed record FieldEntry(
        string ClassName,
        string Namespace,
        string TypeParamList,
        string FieldName,
        ImmutableArray<MethodEntry> Methods);

    /// <summary>
    /// Aggregated model for a single partial class, grouping all its <see cref="FieldEntry"/> records
    /// so that one source file is emitted per class.
    /// </summary>
    /// <param name="ClassName">Simple name of the partial class.</param>
    /// <param name="Namespace">Namespace of the partial class.</param>
    /// <param name="TypeParamList">Generic type-parameter list (e.g. <c>&lt;T&gt;</c>) or empty string.</param>
    /// <param name="Fields">All fields that belong to this class.</param>
    private sealed record ClassModel(
        string ClassName,
        string Namespace,
        string TypeParamList,
        ImmutableArray<FieldEntry> Fields);

    // ── Pipeline ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers the incremental generator pipeline: attribute discovery → extraction →
    /// grouping by class → source emission.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
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

    /// <summary>
    /// Transforms a single <c>[ForwardInterface]</c>-decorated variable declarator into a
    /// <see cref="FieldEntry"/>, or returns <see langword="null"/> if the symbol is invalid
    /// (not a field, not typed as an interface, or produces no method entries).
    /// </summary>
    /// <param name="ctx">Roslyn generator attribute context for the decorated node.</param>
    /// <param name="ct">Cancellation token forwarded from the compilation pipeline.</param>
    /// <returns>A populated <see cref="FieldEntry"/>, or <see langword="null"/>.</returns>
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

    /// <summary>
    /// Builds a <see cref="MethodEntry"/> from a Roslyn <see cref="IMethodSymbol"/>,
    /// rendering all strings needed for source emission (return type, parameters, constraints,
    /// call arguments, and the conflict-detection key).
    /// </summary>
    /// <param name="method">The interface method symbol to model.</param>
    /// <param name="sourceIfaceFqn">
    /// Fully-qualified name of the interface that declared <paramref name="method"/>,
    /// used as the explicit-implementation prefix in case of a signature conflict.
    /// </param>
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

    /// <summary>
    /// Renders a single parameter as a <c>Type name</c> or <c>Type name = default</c> string
    /// suitable for use in a method signature.
    /// </summary>
    /// <param name="p">The parameter symbol to render.</param>
    /// <returns>A source-ready parameter string.</returns>
    private static string RenderParameter(IParameterSymbol p)
    {
        var typeStr = p.Type.ToDisplayString(FullyQualifiedNullable);
        return p.HasExplicitDefaultValue
            ? $"{typeStr} {p.Name}{RenderDefault(p)}"
            : $"{typeStr} {p.Name}";
    }

    /// <summary>
    /// Returns the <c>= value</c> fragment for a parameter's default value, correctly formatted
    /// for enums, strings, booleans, chars, and numeric literals.
    /// </summary>
    /// <param name="p">The parameter symbol whose explicit default value is rendered.</param>
    /// <returns>A source-ready default-value fragment such as <c> = true</c> or <c> = MyEnum.Value</c>.</returns>
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

    /// <summary>
    /// Renders a <c>where TParam : ...</c> constraint clause for the given type parameter,
    /// or returns an empty string if the parameter has no constraints.
    /// </summary>
    /// <param name="tp">The type-parameter symbol to render.</param>
    /// <returns>A <c>where TParam : ...</c> string, or <see cref="string.Empty"/>.</returns>
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

    /// <summary>
    /// Groups a flat list of <see cref="FieldEntry"/> records by their containing class
    /// (namespace + class name + type-parameter list) into <see cref="ClassModel"/> instances,
    /// one per unique class.
    /// </summary>
    /// <param name="entries">All field entries collected across the compilation.</param>
    /// <returns>An immutable array of <see cref="ClassModel"/> records ready for emission.</returns>
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

    /// <summary>
    /// Generates the complete source text for a <c>.Forwarding.g.cs</c> file from a
    /// <see cref="ClassModel"/>. Public forwarding methods are emitted for the first
    /// occurrence of each signature; subsequent occurrences from different interfaces
    /// are emitted as explicit interface implementations to resolve ambiguity.
    /// </summary>
    /// <param name="model">The aggregated class model containing all fields and their methods.</param>
    /// <returns>A source-ready C# file as a string.</returns>
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
