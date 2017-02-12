using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSA.ProxyTree.Nodes
{
    class FieldNode : BasicProxyNode
    {
        public FieldNode(SyntaxNode origin) : base(origin)
        {
            var org = Origin as FieldDeclarationSyntax;
            Debug.Assert(org != null, "org != null");
            Protection = FindProtection(org.Modifiers, "private");
            var varDeclaration = org.ChildNodes().First(x => x.Kind() == SyntaxKind.VariableDeclaration) as VariableDeclarationSyntax;
            Debug.Assert(varDeclaration != null, "varDeclaration != null");
            Type = varDeclaration.Type.ToString();
            Variables = varDeclaration.Variables.Select(x => x.ToString()).ToList();
        }

        public List<string> Variables { get; }

        public string Signature => string.Join(", ", Variables);

        public string Type { get; }

        public string Protection { get; }
    }
}