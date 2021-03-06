﻿using System.Collections.Generic;
using System.IO;
using CSA.ProxyTree.Nodes;
using CSA.ProxyTree.Nodes.Interfaces;
using CSA.ProxyTree.Visitors.Standards;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject;

namespace CSA.ProxyTree.Visitors
{
    class MetricCalculatorVisitor : ApplyDefaultVisitor
    {
        private readonly TextWriter _output;
        private readonly LinkedList<JObject> _results;
        private dynamic _current = null;

        private int _nbNodes = 0;
        private int _nbFiles = 0;
        private int _nbClass = 0;

        public MetricCalculatorVisitor([Named("Metric")] TextWriter output, LinkedList<JObject> results)
        {
            _output = output;

            _results = results;
            _results.Clear();
        }

        public override void Apply(IProxyNode node)
        {
            if (_current == null)
            {
                _current = new JObject();
                _current.NbConditionnals = 0;
                _current.NbLocalVars = 0;
                _current.NbLoops = 0;
                _current.NbQueries = 0;
                _current.NbLambdas = 0;
                _current.NbBreaks = 0;
            }

            ++_nbNodes;
            switch (node.Kind)
            {
                case SyntaxKind.IfStatement:
                case SyntaxKind.SwitchStatement:
                    _current.NbConditionnals += 1;
                    break;

                case SyntaxKind.WhileStatement:
                case SyntaxKind.DoStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.ForStatement:
                    _current.NbLoops += 1;
                    break;

                case SyntaxKind.QueryExpression:
                    _current.NbQueries += 1;
                    break;

                case SyntaxKind.ClassDeclaration:
                    _nbClass++;
                    break;

                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                    _current.NbLambdas += 1;
                    break;

                case SyntaxKind.LocalDeclarationStatement:
                    _current.NbLocalVars += 1;
                    break;

                case SyntaxKind.BreakStatement:
                case SyntaxKind.ContinueStatement:
                case SyntaxKind.GotoStatement:
                case SyntaxKind.GotoCaseStatement:
                case SyntaxKind.GotoDefaultStatement:
                case SyntaxKind.YieldBreakStatement:
                case SyntaxKind.YieldReturnStatement:
                case SyntaxKind.ReturnStatement:
                    _current.NbBreaks += 1;
                    break;
                default:
                    break;
            }
        }

        public override void Apply(ForestNode node)
        {
            _nbFiles = node.Childs.Count;

            dynamic general = new JObject();
            general.Name = "Generic Metrics";
            general.NbFiles = _nbFiles;
            general.NbClass = _nbClass;
            general.NbNodes = _nbNodes;
            _results.AddFirst(general);

            var res = JsonConvert.SerializeObject(_results, Formatting.Indented);
            _output.WriteLine(res);
            _output.Flush();
        }

        public override void Apply(MethodNode node)
        {
            // It's empty, just forget it
            if (_current == null)
                return;

            _current.MethodName = node.Signature;
            _current.ClassName = node.ClassSignature;
            _current.FileName = node.FileName;

            _results.AddLast(_current);
            _current = null;
        }

        public override void Apply(PropertyAccessorNode node)
        {
            // It's empty, just forget it
            if (_current == null)
                return;

            _current.MethodName = node.Signature;
            _current.ClassName = node.ClassSignature;
            _current.FileName = node.FileName;

            _results.AddLast(_current);
            _current = null;
        }
    }
}