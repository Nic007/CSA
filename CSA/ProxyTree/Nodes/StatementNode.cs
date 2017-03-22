using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSA.ProxyTree.Visitors.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ninject;

namespace CSA.ProxyTree.Nodes
{
    public class StatementNode : BasicProxyNode
    {
        public StatementNode(SyntaxNode origin, bool analyzeDataFlow = true) : base(origin)
        {
            LineNumber = origin.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            if (analyzeDataFlow && origin is StatementSyntax)
            {
                // Analyze data flow
                var model = Program.Kernel.Get<SemanticModel>();
                var results = model.AnalyzeDataFlow(origin);
                VariablesDefined = results.WrittenInside.Select(x => x.Name).ToImmutableHashSet();
                VariablesUsed = results.ReadInside.Select(x => x.Name).ToImmutableHashSet();
            }
        }

        public override string ToString()
        {
            return Origin.ToString();
        }

        public int LineNumber { get; }

        public ImmutableHashSet<string> VariablesDefined { get; protected set; }
        public ImmutableHashSet<string> VariablesUsed { get; protected set; }

        public override void Accept(IProxyVisitor visitor) => visitor.Apply(this);
    }
}