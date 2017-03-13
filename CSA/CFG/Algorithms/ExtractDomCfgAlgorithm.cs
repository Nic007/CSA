using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSA.CFG.Iterators;
using CSA.CFG.Nodes;
using CSA.Options;
using DotBuilder;
using DotBuilder.Attributes;
using DotBuilder.Statements;
using Ninject;

namespace CSA.CFG.Algorithms
{
    class ExtractDomCfgAlgorithm : ICfgAlgorithm
    {
        private readonly CfgGraph _cfg;
        private readonly string _outputFolder;
        private readonly string _graphVizPath;

        public ExtractDomCfgAlgorithm([Named("CFG")] CfgGraph graph, ProgramOptions options)
        {
            _cfg = graph;
            _outputFolder = "dom-graph";
            Directory.CreateDirectory(_outputFolder);

            _graphVizPath = options.GraphVizPath;
        }

        public void Execute()
        {
            var classGraphs = new Dictionary<string, GraphBase>();

            foreach (var method in _cfg.CfgMethods.Where(x => x.Value.Root != null))
            {
                GraphBase graph;
                if (!classGraphs.ContainsKey(method.Value.ClassSignature))
                {
                    graph = Graph.Directed("UML").Of(Font.Name("Bitstream Vera Sans"), Font.Size(8))
                        .With(AttributesFor.Node.Of(Shape.Box));
                    classGraphs[method.Value.ClassSignature] = graph;
                }
                graph = classGraphs[method.Value.ClassSignature];

                var subGraph = Subgraph.Cluster;
                subGraph.Of(Label.With(method.Key));
                graph.With(subGraph);

                Execute(method.Value, subGraph);
            }

            var graphviz = new GraphViz(_graphVizPath, OutputFormat.Png);

            foreach (var classGraph in classGraphs)
            {
                var file = new FileStream(_outputFolder + "/" + classGraph.Key + ".png", FileMode.Create);
                graphviz.RenderGraph(classGraph.Value, file);

                // For debug purpose
                var dotFile = classGraph.Value.Render();
                //Console.WriteLine(dotFile);
                using (TextWriter fs = new StreamWriter(_outputFolder + "/" + classGraph.Key + ".dot"))
                {
                    fs.WriteLine(dotFile);
                }
            }

            System.Diagnostics.Process.Start(_outputFolder);
        }

        private void Execute(CfgMethod method, Subgraph subGraph)
        {
            var domParent = new Dictionary<CfgNode, CfgNode>();
            var treeNodes = new HashSet<CfgNode>();

            var treeEnumerator = new PreOrderDepthFirstTreeCfgIterator(method.Root);
            foreach (var link in treeEnumerator.LinkEnumerable)
            {
                domParent[link.To] = link.From;
                treeNodes.Add(link.From);
            }

            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var node in treeEnumerator.NodeEnumerable)
                {
                    if (!node.Prec.Any())
                    {
                        continue;
                    }

                    CfgNode parent = null;
                    foreach (var p in node.Prec)
                    {
                        if (parent == null)
                        {
                            parent = p;
                            continue;
                        }

                        if (!treeNodes.Contains(p))
                        {
                            continue;
                        }

                        parent = FindCommonRoot(domParent, parent, p);
                    }

                    if (domParent[node] != parent)
                    {
                        domParent[node] = parent;
                        changed = true;
                    }
                }
            }

            foreach (var domLink in domParent)
            {
                var child = domLink.Key;
                var parent = domLink.Value;

                var from = Node.Name(parent.UniqueId).Of(Label.With(parent.ToDotString(true)));
                if (parent.Origin == null)
                {
                    @from = @from.Of(Shape.Diamond);
                }

                var to = Node.Name(child.UniqueId).Of(Label.With(child.ToDotString(true)));
                if (child.Origin == null)
                {
                    to = to.Of(Shape.Diamond);
                }

                subGraph.With(@from);
                subGraph.With(to);
                subGraph.With(Edge.Between(parent.UniqueId, child.UniqueId));
            }
        }

        private CfgNode FindCommonRoot(Dictionary<CfgNode, CfgNode> domParent, CfgNode node1, CfgNode node2)
        {
            var path = new HashSet<CfgNode>();
            var current = node1;
            while (current != null)
            {
                path.Add(current);
                current = domParent.ContainsKey(current) ? domParent[current] : null;
            }

            current = node2;
            while (current != null && !path.Contains(current))
            {
                current = domParent.ContainsKey(current) ? domParent[current] : null;
            }

            return current;
        }

        public string Name => GetType().Name;
    }
}
