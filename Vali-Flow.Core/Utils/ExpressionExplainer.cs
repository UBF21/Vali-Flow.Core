using System.Linq.Expressions;
using System.Text;

namespace Vali_Flow.Core.Utils;

/// <summary>
/// Converts an <see cref="Expression"/> tree into a human-readable infix string,
/// used by <see cref="Classes.Base.BaseExpression{TBuilder,T}.Explain"/> for debugging and logging.
/// </summary>
/// <remarks>
/// Binary operators are rendered with standard symbols (AND, OR, ==, &gt;, etc.).
/// <c>NOT</c> prefixes unary negations. Type conversions are transparent (the inner
/// operand is emitted without a cast label). All other unary forms are wrapped in
/// <c>[NodeType]</c> brackets. Extension methods are rendered in instance-call style
/// (<c>receiver.Method(args)</c>). Static non-extension methods show the declaring type name.
/// </remarks>
internal sealed class ExpressionExplainer : ExpressionVisitor
{
    /// <summary>Accumulates the human-readable representation of the expression tree.</summary>
    private readonly StringBuilder _sb = new();

    /// <summary>
    /// Converts <paramref name="expression"/> to a human-readable infix string.
    /// </summary>
    /// <param name="expression">The expression tree to explain.</param>
    /// <returns>A string representation of the expression tree in infix notation.</returns>
    public static string Explain(Expression expression)
    {
        var explainer = new ExpressionExplainer();
        explainer.Visit(expression);
        return explainer._sb.ToString();
    }

    /// <summary>Appends the infix representation of a binary expression surrounded by parentheses.</summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        _sb.Append('(');
        Visit(node.Left);
        _sb.Append(node.NodeType switch
        {
            ExpressionType.AndAlso => " AND ",
            ExpressionType.OrElse => " OR ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.Equal => " == ",
            ExpressionType.NotEqual => " != ",
            ExpressionType.Add => " + ",
            ExpressionType.Subtract => " - ",
            ExpressionType.Multiply => " * ",
            ExpressionType.Divide => " / ",
            _ => $" {node.NodeType} "
        });
        Visit(node.Right);
        _sb.Append(')');
        return node;
    }

    /// <summary>
    /// Appends the infix representation of a unary expression.
    /// <c>Not</c> is rendered as <c>NOT operand</c>; <c>Convert</c> and <c>ConvertChecked</c> are transparent;
    /// all other node types are wrapped in <c>[NodeType]operand</c>.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType == ExpressionType.Not)
        {
            _sb.Append("NOT ");
            Visit(node.Operand);
        }
        else if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
        {
            Visit(node.Operand);
        }
        else
        {
            _sb.Append($"[{node.NodeType}]");
            Visit(node.Operand);
        }
        return node;
    }

    /// <summary>Appends a member access in <c>receiver.MemberName</c> form; omits the receiver prefix for static members.</summary>
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null)
        {
            Visit(node.Expression);
            _sb.Append('.');
        }
        _sb.Append(node.Member.Name);
        return node;
    }

    /// <summary>Appends the parameter's name, or <c>&lt;param&gt;</c> when the name is unavailable.</summary>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        _sb.Append(node.Name ?? "<param>");
        return node;
    }

    /// <summary>
    /// Appends the constant value: strings are double-quoted, <see langword="null"/> is written as <c>null</c>,
    /// and all other values use their <see cref="object.ToString()"/> representation.
    /// </summary>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value is string s)
        {
            _sb.Append($"\"{s}\"");
        }
        else if (node.Value is null)
        {
            _sb.Append("null");
        }
        else
        {
            _sb.Append(node.Value.ToString());
        }
        return node;
    }

    /// <summary>
    /// Appends a method call.
    /// Extension methods are rendered as <c>receiver.Method(args)</c>.
    /// Instance methods use the instance as the receiver.
    /// Static non-extension methods use the declaring type name as the receiver.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        bool isExtension = node.Object == null
            && node.Arguments.Count > 0
            && node.Method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);

        if (node.Object != null)
        {
            Visit(node.Object);
        }
        else if (isExtension)
        {
            Visit(node.Arguments[0]);
        }
        else
        {
            _sb.Append(node.Method.DeclaringType?.Name);
        }
        _sb.Append($".{node.Method.Name}(");
        int start = (node.Object != null || !isExtension) ? 0 : 1;
        for (int i = start; i < node.Arguments.Count; i++)
        {
            if (i > start) _sb.Append(", ");
            Visit(node.Arguments[i]);
        }
        _sb.Append(')');
        return node;
    }
}
