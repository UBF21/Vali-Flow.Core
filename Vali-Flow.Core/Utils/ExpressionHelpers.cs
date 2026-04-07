using System.Linq.Expressions;

namespace Vali_Flow.Core.Utils;

/// <summary>Shared expression visitor utilities used internally by expression builder classes.</summary>
internal static class ExpressionHelpers
{
    /// <summary>
    /// Replaces all occurrences of a source parameter with a replacement expression
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
    /// Handles all common node types: Member, Unary, Binary, MethodCall, Index, Conditional, New, and Lambda.
    /// <para>
    /// <b>VisitParameter is intentionally NOT overridden.</b> ParameterExpression nodes are immutable
    /// value-like objects — sharing them between the original and cloned tree is correct and expected.
    /// The caller (e.g. BuildNullSafeCollectionPredicate) uses selector.Parameters[0] as the single
    /// root parameter for the combined lambda, so all ParameterExpression references inside the cloned
    /// body must point to that same root instance. Overriding VisitParameter to create new instances
    /// breaks parameter resolution and fails ~100 tests.
    /// </para>
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

        protected override Expression VisitIndex(IndexExpression node)
        {
            var obj = Visit(node.Object)!;
            var args = node.Arguments.Select(a => Visit(a)!);
            return node.Indexer != null
                ? Expression.Property(obj, node.Indexer, args)
                : Expression.ArrayAccess(obj, args);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var test = Visit(node.Test)!;
            var ifTrue = Visit(node.IfTrue)!;
            var ifFalse = Visit(node.IfFalse)!;
            return Expression.Condition(test, ifTrue, ifFalse, node.Type);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var args = node.Arguments.Select(a => Visit(a)!);
            return node.Members != null
                ? Expression.New(node.Constructor!, args, node.Members)
                : Expression.New(node.Constructor!, args);
        }

        protected override Expression VisitLambda<TDelegate>(Expression<TDelegate> node)
        {
            var body = Visit(node.Body)!;
            var parameters = node.Parameters.Select(p => (ParameterExpression)Visit(p)!).ToArray();
            return Expression.Lambda<TDelegate>(body, parameters);
        }
    }
}
