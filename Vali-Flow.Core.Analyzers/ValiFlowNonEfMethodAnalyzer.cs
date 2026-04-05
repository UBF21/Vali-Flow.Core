using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Vali_Flow.Core.Analyzers;

/// <summary>
/// Roslyn analyzer VF001: warns when a ValiFlowQuery method that is not translatable by
/// EF Core is called on a ValiFlowQuery&lt;T&gt; instance.
/// </summary>
/// <remarks>
/// These methods generate in-memory predicates (Regex, StringComparison, Enumerable.All/Any
/// with lambdas, GroupBy, Distinct, etc.) that Entity Framework Core cannot translate to SQL.
/// Using them inside a ValiFlowQuery — which is designed for IQueryable scenarios — will
/// cause EF Core to silently fall back to client-side evaluation or throw at runtime.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValiFlowNonEfMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "VF001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ValiFlowQuery method is not translatable by EF Core",
        messageFormat: "'{0}' is not translatable by EF Core. " +
                       "Use ValiFlow<T> (in-memory) instead, or replace with an EF Core-compatible expression.",
        category: "ValiFlow.EFCoreCompatibility",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ValiFlowQuery<T> is designed for EF Core IQueryable queries. " +
                     "Methods that rely on Regex, StringComparison, LINQ operators over characters, " +
                     "or Enumerable.All/Any with predicates cannot be translated to SQL and will " +
                     "cause client-side evaluation or a runtime exception.",
        helpLinkUri: "https://github.com/UBF21/Vali-Flow.Core#ef-core-compatibility");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    // Methods on ValiFlowQuery<T> that are NOT EF Core translatable.
    // This list mirrors the methods documented with "not EF Core translatable" in the library.
    private static readonly ImmutableHashSet<string> NonEfMethods = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        // IStringFormatExpression — all regex-based
        "IsEmail",
        "IsUrl",
        "IsPhoneNumber",
        "IsGuid",
        "IsJson",
        "NotJson",
        "IsBase64",
        "NotBase64",
        "RegexMatch",
        "MatchesWildcard",
        "IsCreditCard",
        "IsIPv4",
        "IsIPv6",
        "IsHexColor",
        "IsSlug",
        // IStringStateExpression — char-level LINQ
        "IsTrimmed",
        "IsLowerCase",
        "IsUpperCase",
        "HasOnlyDigits",
        "HasOnlyLetters",
        "HasLettersAndNumbers",
        "HasSpecialCharacters",
        // IStringContentExpression — StringComparison / ToLower
        "EqualToIgnoreCase",
        "IsOneOf",
        // ICollectionExpression — predicate lambdas / Distinct / GroupBy
        "All",
        "Any",
        "None",
        "DistinctCount",
        "HasDuplicates",
        "EachItem",
        "AnyItem",
        "AllMatch"
    );

    // Fully-qualified type names of ValiFlowQuery (both generic and open-generic form).
    private const string ValiFlowQueryTypeName = "ValiFlowQuery";
    private const string ValiFlowQueryFullName = "Vali_Flow.Core.Builder.ValiFlowQuery";

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // We only care about member access expressions: foo.MethodName(...)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var methodName = memberAccess.Name.Identifier.Text;
        if (!NonEfMethods.Contains(methodName))
        {
            return;
        }

        // Resolve the symbol to check whether it belongs to ValiFlowQuery<T>
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        ISymbol? resolved = symbolInfo.Symbol;
        if (resolved == null && symbolInfo.CandidateSymbols.Length > 0)
        {
            resolved = symbolInfo.CandidateSymbols[0];
        }

        var methodSymbol = resolved as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        // Walk up the containing type chain — the method may be defined on a base class or
        // implemented interface, so we check the receiver type of the invocation.
        var receiverType = context.SemanticModel
            .GetTypeInfo(memberAccess.Expression, context.CancellationToken)
            .Type;

        if (receiverType == null)
        {
            return;
        }

        if (!IsValiFlowQueryType(receiverType))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Name.GetLocation(),
            methodName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsValiFlowQueryType(ITypeSymbol type)
    {
        // Check the type itself and every type in its inheritance chain
        var current = type;
        while (current != null)
        {
            if (MatchesValiFlowQuery(current))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static bool MatchesValiFlowQuery(ITypeSymbol type)
    {
        // Match by short name OR full metadata name (covers generic and non-generic)
        if (type.Name == ValiFlowQueryTypeName)
        {
            return true;
        }

        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullName.Contains(ValiFlowQueryFullName);
    }
}
