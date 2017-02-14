﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSA.ProxyTree.Iterators;
using CSA.ProxyTree.Nodes;
using Ninject;

namespace CSA.ProxyTree.Algorithms
{
    class InitTreeAlgorithm : IProxyAlgorithm
    {
        private ClassNode _currentClass;
        private PropertyNode _currentProperty;
        private readonly List<Func<IProxyNode, bool>> _filterRules; 

        public InitTreeAlgorithm([Named("PreOrder")] IProxyIterator iterator)
        {
            Iterator = iterator;

            _filterRules = new List<Func<IProxyNode, bool>>
            {
                node => !node.GetType().IsSubclassOf(typeof (BasicProxyNode)),
                node => (node is ExpressionNode) && _currentClass == null
            };
        }

        public IProxyIterator Iterator { get; }
        public void Apply(IProxyNode node)
        {
            if (TrimTree && _filterRules.Any(x => x(node)))
            {
                // Thoses nodes are not concrete and should be filtered out

                // Break the relation
                var ndx = node.Parent.Childs.FindIndex(x => x == node);
                node.Parent.Childs.RemoveAt(ndx);
                node.Parent.Childs.InsertRange(ndx, node.Childs);
                node.Childs.ForEach(x => x.Parent = node.Parent);


                if (node.Left != null)
                    node.Left.Right = node.Right;
                if (node.Right != null)
                    node.Right.Left = node.Left;
            }
            // We keep theses nodes
            else
            {
                node.ClassSignature = _currentClass.Signature;
            }
        }

        public bool TrimTree => true;

        public void Apply(ForestNode node)
        {
            // Nothing to do
        }

        public void Apply(ClassNode node)
        {
            _currentClass = node;

            Apply(node as IProxyNode);
        }

        public void Apply(MethodNode node)
        {
            Apply(node as IProxyNode);
        }

        public void Apply(PropertyNode node)
        {
            _currentProperty = node;

            Apply(node as IProxyNode);
        }

        public void Apply(PropertyAccessorNode node)
        {
            node.Type = _currentProperty.Type;
            if (node.Protection == "")
            {
                node.Protection = _currentProperty.Protection;
            }

            node.Name = _currentProperty.Signature;

            Apply(node as IProxyNode);
        }
    }
}