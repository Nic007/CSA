﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSA.ProxyTree.Algorithms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSA.ProxyTree.Nodes
{
    public class BasicProxyNode : IProxyNode
    {
        protected SyntaxNode Origin { get; }

        public IProxyNode Parent { get; set; }
        public IProxyNode Left { get; set; }
        public IProxyNode Right { get; set; }

        public List<IProxyNode> Childs { get; }

        public SyntaxKind Kind => Origin.Kind();

        public BasicProxyNode(SyntaxNode origin)
        {
            Origin = origin;
            Childs = new List<IProxyNode>();
        }

        public string FileName => Origin.SyntaxTree.FilePath;

        public IEnumerable<IProxyNode> Ancestors()
        {
            var curr = Parent;
            while (curr != null)
            {
                yield return curr;
                curr = curr.Parent;
            }
        }

        public virtual void Accept(IProxyAlgorithm algorithm) => algorithm.Apply(this);

        public string ClassSignature { get; set; }

        protected string FindProtection(SyntaxTokenList modifiers, string def)
        {
            try
            {
                var modifier = modifiers.First(x =>
                    x.Kind() == SyntaxKind.PublicKeyword ||
                    x.Kind() == SyntaxKind.PrivateKeyword ||
                    x.Kind() == SyntaxKind.ProtectedKeyword ||
                    x.Kind() == SyntaxKind.InternalKeyword);
                return modifier.ToString();
            }
            catch (InvalidOperationException)
            {
                return def;
            }
        }
    }
}
