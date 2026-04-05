using System.Linq.Expressions;
using System.Text;

namespace Vali_Flow.Core.Utils;

internal sealed class ExpressionExplainer : ExpressionVisitor
{
    private readonly StringBuilder _sb = new();

    public static string Explain(Expression expression)
    {
        var explainer = new ExpressionExplainer();
        explainer.Visit(expression);
        return explainer._sb.ToString();
    }

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

    protected override Expression VisitParameter(ParameterExpression node)
    {
        _sb.Append(node.Name ?? "<param>");
        return node;
    }

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
        else if (node.Arguments.Count > 0)
        {
            _sb.Append(node.Method.DeclaringType?.Name);
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
