using System.Linq.Expressions;

namespace Vali_Flow.Core.Utils;

/// <summary>Shared expression visitor utilities used internally by expression builder classes.</summary>
internal static class ExpressionHelpers
{
    /// <summary>
    /// Replaces all occurrences of <paramref name="old"/> with <paramref name="new"/>
    /// in an expression tree.
    /// </summary>
    internal sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly Expression _new;

        internal ParameterReplacer(ParameterExpression old, Expression @new)
        {
            _old = old;
            _new = @new;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _old ? _new : base.VisitParameter(node);
    }

    /// <summary>
    /// Produces a structurally independent (reference-distinct) copy of any expression subtree
    /// so that the same expression body can appear in two positions in a compound expression tree
    /// without sharing nodes (e.g., null-check AND predicate call on the same selector body).
    /// Handles MemberExpression, UnaryExpression, BinaryExpression, and MethodCallExpression.
    /// </summary>
    internal sealed class ForceCloneVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var expr = Visit(node.Expression);
            return Expression.MakeMemberAccess(expr, node.Member);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand)!;
            return Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left)!;
            var right = Visit(node.Right)!;
            return node.Conversion != null
                ? Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method,
                    (LambdaExpression)Visit(node.Conversion)!)
                : Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = node.Object != null ? Visit(node.Object) : null;
            var args = node.Arguments.Select(a => Visit(a)!);
            return Expression.Call(obj, node.Method, args);
        }
    }
}
