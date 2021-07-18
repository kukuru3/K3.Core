using System;
using System.Collections.Generic;
using System.Linq;

namespace K3.Graphs {

    /// <summary>The graph is bidirectional</summary>
    public class Graph {

        protected List<Node> nodes = new List<Node>();
        protected List<Edge> edges = new List<Edge>();

        public void Clear() {
            nodes.Clear();
            edges.Clear();
        }

        protected virtual NodeConstructor NodeConstructor => (g) => new Node(g);
        protected virtual EdgeConstructor EdgeConstructor => (a,b) => new Edge(a, b);

        internal Edge Connect(Node a, Node b) {
            if (a == null || b == null) throw new InvalidGraphException($"Can't connect null node; a={a}, b={b}");
            var e = EdgeConstructor(a, b);
            edges.Add(e);
            return e;
        }

        internal Edge GetEdge(Node a, Node b) {
            if (a == null || b == null) return default;
            foreach (var edge in a.edges) if (edge.Connects(b)) return edge;
            return default;
        }

        internal Node CreateNode() {
            var n = NodeConstructor(this);
            nodes.Add(n);
            return n;
        }

        protected E GetEdge<E, N>(N a, N b) where E : Edge where N : Node => (E)GetEdge(a,b);
        protected E Connect<E, N>(N a, N b) where E : Edge where N : Node => (E)Connect(a,b);
        protected T CreateNode<T>() where T : Node => (T)CreateNode();

        public bool AreConected(Node a, Node b) => GetEdge(a,b) != null;

        public void Disconnect(Edge e) {
            if (edges.Remove(e)) { 
                e.OnRemoved();
                e.a.edges.Remove(e);
                e.b.edges.Remove(e);
            } else throw new InvalidGraphException($"Can't disconnect {e} - not in graph");
        }

        public void RemoveNode(Node n) {
            if (nodes.Remove(n)) {
                n.OnRemoved();
                var copyOfEdges = n.edges;
                foreach (var edge in copyOfEdges) Disconnect(edge);
            } else throw new InvalidGraphException($"Can't remove {n} - not in graph");
        }

        public void PruneGraph() {
            var toRemove = nodes.Where(n => n.edges.Count == 0).ToArray();
            foreach (var orphan in toRemove) RemoveNode(orphan);
        }
    }

    class InvalidGraphException : System.Exception {
        public InvalidGraphException(string msg) : base(msg) { }
    }

    public delegate Node NodeConstructor(Graph g);
    public delegate Edge EdgeConstructor(Node a, Node b);

    public class Node {
        public readonly Graph graph;

        internal List<Edge> edges;

        public IEnumerable<Edge> Edges => edges;

        public Node(Graph g) {
            this.graph = g;
            edges = new List<Edge>();
        }

        protected internal virtual void OnRemoved() { }
    }

    public class Edge {
        public readonly Node a;
        public readonly Node b;

        public Edge(Node a, Node b) {
            this.a = a;
            this.b = b;
        }

        public bool Connects(Node n) => a == n || b == n;
        protected internal virtual void OnRemoved() { }
    }

    
}