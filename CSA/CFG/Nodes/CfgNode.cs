﻿using System;
using System.Collections.Generic;
using CSA.CFG.Algorithms;
using CSA.CFG.Iterators;
using CSA.FixPoint;
using CSA.ProxyTree.Nodes;
using Microsoft.CodeAnalysis.CSharp;

namespace CSA.CFG.Nodes
{
    public class CfgNode : IFixPointAnalyzableNode
    {
        private readonly string _name;
        private static int _instance = 0;

        public CfgNode(StatementNode origin)
        {
            Origin = origin;
            Prec = new HashSet<CfgNode>();
            Next = new HashSet<CfgNode>();
            UniqueId = _instance++.ToString();
        }

        public CfgNode(string name)
        {
            _name = name;
            Origin = null;
            Prec = new HashSet<CfgNode>();
            Next = new HashSet<CfgNode>();
            UniqueId = _instance++.ToString();
        }

        public StatementNode Origin {get;}

        public SyntaxKind Kind => Origin?.Kind ?? SyntaxKind.None;

        public HashSet<CfgNode> Prec { get; }

        public HashSet<CfgNode> Next { get; }

        public string UniqueId { get; }

        public string ToDotString(bool withLineNumber = false)
        {
            var text = _name ?? Origin.ToString();
            text += Environment.NewLine + $"UniqueId: {UniqueId}";
            if (Origin != null && withLineNumber)
            {
                text += Environment.NewLine + $"Line: {Origin.LineNumber}";
            }
            return text.Replace(System.Environment.NewLine, @"\n").Replace("\"", " \\\"");
        }

        public string ToDotString(string more, bool withLineNumber = false)
        {
            var text = ToDotString(withLineNumber);
            if (more != null)
            {
                text += Environment.NewLine + more;
            }
            return text.Replace(System.Environment.NewLine, @"\n").Replace("\"", " \\\"");
        }

        public IEnumerable<CfgNode> NodeEnumerator => (new PreOrderDepthFirstCfgIterator(this)).NodeEnumerable;
        public IEnumerable<CfgLink> LinkEnumerator => (new PreOrderDepthFirstCfgIterator(this)).LinkEnumerable;

        IEnumerable<IFixPointAnalyzableNode> IFixPointAnalyzableNode.Prec() => Prec;
        IEnumerable<IFixPointAnalyzableNode> IFixPointAnalyzableNode.Succ() => Next;

        public override bool Equals(object obj)
        {
            CfgNode other = obj as CfgNode;
            if (other != null)
            {
                return UniqueId == other.UniqueId;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return UniqueId.GetHashCode();
        }
    }
}