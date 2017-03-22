using System.Collections.Immutable;
using System.Diagnostics;
using CSA.ProxyTree.Visitors.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSA.ProxyTree.Nodes.Statements
{
    public class GotoStatementNode : StatementNode
    {
        public GotoStatementNode(SyntaxNode origin) : base(origin, false)
        {
            var stmt = Origin as GotoStatementSyntax;
            Debug.Assert(stmt != null, "stmt != null");
            Label = stmt.Expression.ToString();
            Target = null;

            VariablesDefined = ImmutableHashSet<string>.Empty;
            VariablesUsed = ImmutableHashSet<string>.Empty;
        }

        public string Label { get; }

        public LabelStatementNode Target { get; set; }

        public override void Accept(IProxyVisitor visitor) => visitor.Apply(this);
    }
}