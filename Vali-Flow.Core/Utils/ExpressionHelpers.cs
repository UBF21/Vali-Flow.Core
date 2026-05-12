using System.Linq.Expressions;

namespace Vali_Flow.Core.Utils;

/// <summary>Shared expression visitor utilities used internally by expression builder classes.</summary>
internal static class ExpressionHelpers
{
    /// <summary>
    /// Replaces all occurrences of a specific <see cref="ParameterExpression"/> with an arbitrary
    /// replacement <see cref="Expression"/> throughout an expression tree.
    /// </summary>
    /// <remarks>
    /// Used whenever two independently built expressions need to share a single parameter instance —
    /// for example, when composing a selector body and a predicate body into one combined lambda.
    /// </remarks>
    internal sealed class ParameterReplacer : ExpressionVisitor
    {
        /// <summary>The parameter expression to find and replace.</summary>
        private readonly ParameterExpression _old;

        /// <summary>The expression to substitute in place of every occurrence of <see cref="_old"/>.</summary>
        private readonly Expression _new;

        /// <summary>
        /// Initializes a new <see cref="ParameterReplacer"/>.
        /// </summary>
        /// <param name="old">The parameter to replace.</param>
        /// <param name="new">The expression to substitute.</param>
        internal ParameterReplacer(ParameterExpression old, Expression @new)
        {
            _old = old;
            _new = @new;
        }

        /// <summary>Substitutes <see cref="_new"/> when <paramref name="node"/> is the tracked parameter; otherwise delegates to the base visitor.</summary>
        /// <param name="node">The parameter node currently being visited.</param>
        /// <returns><see cref="_new"/> if <paramref name="node"/> equals <see cref="_old"/>; the original node otherwise.</returns>
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
        /// <summary>Creates a structurally independent copy of a <see cref="MemberExpression"/> node.</summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            var expr = Visit(node.Expression);
            return Expression.MakeMemberAccess(expr, node.Member);
        }

        /// <summary>Creates a structurally independent copy of a <see cref="UnaryExpression"/> node.</summary>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand)!;
            return Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method);
        }

        /// <summary>Creates a structurally independent copy of a <see cref="BinaryExpression"/> node, preserving any conversion lambda.</summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left)!;
            var right = Visit(node.Right)!;
            return node.Conversion != null
                ? Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method,
                    (LambdaExpression)Visit(node.Conversion)!)
                : Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
        }

        /// <summary>Creates a structurally independent copy of a <see cref="MethodCallExpression"/> node.</summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = node.Object != null ? Visit(node.Object) : null;
            var args = node.Arguments.Select(a => Visit(a)!);
            return Expression.Call(obj, node.Method, args);
        }

        /// <summary>Creates a structurally independent copy of an <see cref="IndexExpression"/> node, dispatching to array-access or property-indexer form as appropriate.</summary>
        protected override Expression VisitIndex(IndexExpression node)
        {
            var obj = Visit(node.Object)!;
            var args = node.Arguments.Select(a => Visit(a)!);
            return node.Indexer != null
                ? Expression.Property(obj, node.Indexer, args)
                : Expression.ArrayAccess(obj, args);
        }

        /// <summary>Creates a structurally independent copy of a <see cref="ConditionalExpression"/> (ternary) node.</summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var test = Visit(node.Test)!;
            var ifTrue = Visit(node.IfTrue)!;
            var ifFalse = Visit(node.IfFalse)!;
            return Expression.Condition(test, ifTrue, ifFalse, node.Type);
        }

        /// <summary>Creates a structurally independent copy of a <see cref="NewExpression"/> node, preserving member bindings when present.</summary>
        protected override Expression VisitNew(NewExpression node)
        {
            var args = node.Arguments.Select(a => Visit(a)!);
            return node.Members != null
                ? Expression.New(node.Constructor!, args, node.Members)
                : Expression.New(node.Constructor!, args);
        }

        /// <summary>Creates a structurally independent copy of a <see cref="LambdaExpression"/> node.</summary>
        /// <typeparam name="TDelegate">The delegate type of the lambda.</typeparam>
        protected override Expression VisitLambda<TDelegate>(Expression<TDelegate> node)
        {
            var body = Visit(node.Body)!;
            var parameters = node.Parameters.Select(p => (ParameterExpression)Visit(p)!).ToArray();
            return Expression.Lambda<TDelegate>(body, parameters);
        }
    }
}
