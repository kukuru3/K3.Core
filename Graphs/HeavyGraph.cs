using System;
using System.Collections.Generic;
using System.Linq;

namespace K3.Graphs {
    /// <summary>Generic graph, with bidirectional edges. Allows cycles.</summary>
    /// <typeparam name="N">Node type</typeparam>
    /// <typeparam name="E">Edge type</typeparam>
    public abstract class HeavyGraph<N, E> {

        List<Node> nodes = new();
        List<Edge> edges = new();
        Dictionary<N, Node> nodeLookup = new();
        Dictionary<E, Edge> edgeLookup = new();

        public IReadOnlyList<Node> AllNodes => nodes;
        public IReadOnlyList<Edge> AllEdges => edges;

        public IEnumerable<N> NeighboursOf(N node) {
            var n = nodeLookup[node];
            foreach (var e in n.Edges) yield return e.Other(n).data;
        }

        public IEnumerable<(E edge, N neighbour)> EdgesOf(N node) {
            var n = nodeLookup[node];
            foreach (var e in n.Edges) yield return (e.data, e.Other(n).data);
        }

        protected Node CreateNode(N data) {
            var nn = new Node(data);
            nodes.Add(nn);
            nodeLookup.Add(data, nn);
            return nn;
        }

        protected Edge CreateEdge(E e, N a, N b) {
            if (TryGetEdge(a, b) != null) throw new System.Exception("Edge already exists");
            Node nb = null;
            if (nodeLookup.TryGetValue(a, out var na) && nodeLookup.TryGetValue(b, out nb)) { 
                var edge = new Edge(e, na, nb);
                edges.Add(edge);
                na.RegisterEdge(edge);
                nb.RegisterEdge(edge);
                edgeLookup.Add(e, edge);
                return edge;
            } else {
                throw new System.InvalidOperationException($"How create edge? Node not inserted in graph? {a} has value= {na != null}, {b} has value = {nb != null}");
            }
        }

        protected void RemoveEdge(E e) {
            if (edgeLookup.TryGetValue(e, out var edge)) {
                edge.a.Unregister(edge);
                edge.b.Unregister(edge);
                edgeLookup.Remove(e);
                edges.Remove(edge);
            }
        }

        public Node NodeOf(N data) {
            nodeLookup.TryGetValue(data, out var node);
            return node;
        }

        public void RemoveNodes(IEnumerable<Node> nodesToRemove) {
            var set = (nodesToRemove as ISet<Node>) ?? new HashSet<Node>(nodesToRemove);

            var edgesToRemove = new HashSet<Edge>();

            var newList = new List<Node>();
            foreach (var node in nodesToRemove) {
                edgesToRemove.UnionWith(node.Edges);
                nodeLookup.Remove(node.data);
            }

            foreach (var n in nodes) {
                if (!nodesToRemove.Contains(n)) newList.Add(n);
            }
            nodes = newList;

            int[] nodeRemapIndices = new int[nodes.Count];
            for (var i = edges.Count-1; i >= 0; i--) {
                if (edgesToRemove.Contains(edges[i])) {
                    edges[i].a.Unregister(edges[i]);
                    edges[i].b.Unregister(edges[i]);
                    edgeLookup.Remove(edges[i].data);
                    edges.RemoveAt(i);
                }
            }
        }


        public Edge TryGetEdge(Node a, Node b) {
            return a.FindEdgeTo(b);
        }

        public Edge TryGetEdge(N a, N b) {
            if (nodeLookup.TryGetValue(a, out var nodeA) && nodeLookup.TryGetValue(b, out var nodeB)) return nodeA.FindEdgeTo(nodeB);
            return null;
        }

        public struct GraphData {
            public Node parent;
            public int  distance;
            public int  regionID;

            public bool _closed;
        }

        public class Node {
            public readonly N data;
            List<Edge> edges = new();

            // graph data:
            public GraphData graphData;

            public override string ToString() {
                return $"{graphData.regionID}|{graphData.distance}:{data}";
            }

            public Node(N data) {
                this.data = data;
            }

            public IReadOnlyCollection<Edge> Edges => edges;

            public IEnumerable<Node> Neighbours() {
                foreach (var edge in edges) yield return edge.Other(this);
            }

            public Edge FindEdgeTo(Node other) {
                foreach (var edge in edges) if (edge.Other(this) == other) return edge;
                return null;
            }

            public void RegisterEdge(Edge e) {
                edges.Add(e);
            }

            public void Unregister(Edge e) {
                edges.Remove(e);
            }
        }

        public class Edge {

            public override string ToString() => $"{data}";

            public readonly E data;

            public readonly Node a;
            public readonly Node b;

            internal Edge(E data, Node a, Node b)
            {
                this.a = a;
                this.b = b;
                this.data = data;
            }

            public bool Connects(Node n) => n == a || n == b;

            public Node Other(Node n) => n == a ? b : a;
        }

    }
}