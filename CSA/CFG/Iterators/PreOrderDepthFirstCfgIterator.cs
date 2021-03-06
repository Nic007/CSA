﻿using System.Collections.Generic;
using System.Linq;
using CSA.CFG.Nodes;

namespace CSA.CFG.Iterators
{
    class PreOrderDepthFirstCfgIterator : ICfgIterator
    {
        private readonly CfgNode _root;
        private readonly Stack<CfgNode> _stack;
        private readonly HashSet<CfgNode> _visited;

        public PreOrderDepthFirstCfgIterator(CfgNode root)
        {
            _root = root;
            _stack = new Stack<CfgNode>();
            _visited = new HashSet<CfgNode>();
        }

        private bool Accept(CfgNode node) => !_visited.Contains(node);

        public IEnumerable<CfgLink> LinkEnumerable
        {
            get
            {
                _stack.Clear();
                _visited.Clear();
                _stack.Push(_root);

                while (_stack.Any())
                {
                    // Find the current element
                    var current = _stack.Pop();

                    // Find the next elements
                    foreach (var next in current.Next)
                    {
                        // Return the current path
                        yield return new CfgLink(current, next);

                        if (Accept(next))
                        {
                            _stack.Push(next);
                            _visited.Add(next);
                        }
                    }
                }
            }
        }

        public IEnumerable<CfgNode> NodeEnumerable
        {
            get
            {
                _stack.Clear();
                _visited.Clear();
                _stack.Push(_root);

                while (_stack.Any())
                {
                    // Find the current element
                    var current = _stack.Pop();

                    // Find the next elements
                    foreach (var next in current.Next.Where(Accept))
                    {
                        // Return the current path
                        _stack.Push(next);
                        _visited.Add(next);
                    }

                    yield return current;
                }
            }
        }
    }
}