using CSA.ProxyTree.Visitors.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSA.ProxyTree.Nodes.Statements
{
    public class ThrowStatementNode : StatementNode
    {
        public ThrowStatementNode(SyntaxNode origin) : base(origin)
        {
        }

        public override void Accept(IProxyVisitor visitor) => visitor.Apply(this);
    }
}