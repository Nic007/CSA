﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSA.CFG.Iterators;
using CSA.CFG.Nodes;
using Ninject;

namespace CSA.CFG.Algorithms
{
    class ControlDepencies
    {
        public ControlDepencies()
        {
            Graphs = new Dictionary<CfgMethod, HashSet<CfgLink>>();
        }

        public Dictionary<CfgMethod, HashSet<CfgLink>> Graphs { get; }

        public HashSet<CfgLink> this[CfgMethod method]
        {
            get
            {
                if (!Graphs.ContainsKey(method))
                {
                    Graphs[method] = new HashSet<CfgLink>();
                }

                return Graphs[method];
            }
        } 
    }

    class ControlDepecencyAlgorithm : IProduceArtefactsAlgorithm
    {
        public void Execute()
        {
            var domTrees = Program.Kernel.Get<ForestDomTree>("PDomTree");

            var cd = Program.Kernel.Get<ControlDepencies>("ControlDepedency");

            foreach (var domTree in domTrees.Forest)
            {
                Execute(domTree.Value, cd[domTree.Key]);
            }
        }

        private void Execute(IDomTree domTree, HashSet<CfgLink> controlDepedencyLinks)
        {
            var links = domTree.Method.Root.LinkEnumerator.Where(link => !domTree.Dominate(link.To, link.From)).ToList();
            foreach (var link in links)
            {
                var node = link.To;
                do
                {
                    controlDepedencyLinks.Add(new CfgLink(link.From, node));

                    node = domTree[node];
                } while (node != domTree[link.From]);
            }
        }

        public string Name => GetType().Name;

        public IList<string> Artifacts => new List<string> { CSA.Artifacts.ControlDepedency };
        public IList<string> Depedencies => new List<string>{ CSA.Artifacts.PostDomTree };
    }
}
