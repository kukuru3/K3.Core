using System;
using System.Collections.Generic;
namespace K3.Collections {

    public interface IGraphNode<N, E> {
        public Graph<N, E> Graph { get; set; }

        public IEnumerable<N> Neighbours() {
            return Graph.GetNeighbours((N)this);
        }

        public IEnumerable<(E edge, N node)> EnumerateNeighbours() {
            var thisNode = (N)this;
            foreach (var e in Graph.GetEdges(thisNode))
                yield return (e, Graph.GetOther(e, thisNode));
        }
    }

    public interface IGraphEdge<N, E> {
        public Graph<N, E> Graph { get; set; }

        public N Other(N node) => Graph.GetOther((E)this, node);
        public N From => Graph.GetEdgeNode((E)this, 0); 
        public N To => Graph.GetEdgeNode((E)this, 1); 

    }

    /// <summary>Specialized graph. Bidirectional, single-edge-per pair, no self-edges.
    /// This is not guaranteed to be optimal and probably abysmally fits most use cases. But I find myself writing a certain type of graph 
    /// often and for speed of development it felt appropriate to have a ready-made collection at hand.
    /// "performance" is achieved with the "just use a hashmap bro" approach :D :D :D </summary>
    /// <typeparam name="N">type of node.</typeparam>
    /// <typeparam name="E">type of edge</typeparam>
    public class Graph<N, E> {
        public delegate N NodeConstructor();
        public delegate E EdgeConstructor(N a, N b);

        List<N> nodes = new();
        List<E> edges = new();

        NodeConstructor nodeConstructor;
        EdgeConstructor edgeConstructor;

        HashSet<HashableEdge> _fastEdgeSet = new();

        Dictionary<N, HashSet<HashableEdge>> _perNodeEdges = new();
        Dictionary<E, HashableEdge> _edgeLookup = new();

        public Graph(NodeConstructor nodeConstructor, EdgeConstructor edgeConstructor) {
            this.nodeConstructor = nodeConstructor;
            this.edgeConstructor = edgeConstructor;
        }

        public N CreateNode() {
            var n = nodeConstructor();
            nodes.Add(n);
            _perNodeEdges.Add(n, new HashSet<HashableEdge>());
            if (n is IGraphNode<N, E> gn) gn.Graph = this;
            return n;
        }
        
        bool Contains(N node) => _perNodeEdges.ContainsKey(node);

        public IEnumerable<N> GetNeighbours(N node) {
            foreach (var e in _perNodeEdges[node]) yield return e.Other(node);
        }

        public IEnumerable<E> GetEdges(N node) {
            foreach (var e in _perNodeEdges[node]) yield return e.e;
        }

        public IEnumerable<N> AllNodes() => nodes;

        public IEnumerable<E> AllEdges() => edges;
        

        public bool Remove(N node) {
            if (!Contains(node))  return false;
            _fastEdgeSet.RemoveWhere(n => n.Has(node));
            foreach (var e in _perNodeEdges[node])
                _perNodeEdges[e.Other(node)].Remove(e);

            _perNodeEdges.Remove(node);
            nodes.Remove(node);
            return true;

        }

        E _DoConnect(N a, N b) {
            var e = edgeConstructor(a, b);
            var edgeContainer = new HashableEdge(a,b,e);
            _fastEdgeSet.Add(edgeContainer);
            _perNodeEdges[a].Add(edgeContainer);
            _perNodeEdges[b].Add(edgeContainer);
            _edgeLookup[e] = edgeContainer;
            edges.Add(e);
            if (e is IGraphEdge<N, E> ge) ge.Graph = this;
            return e;
        }

        void _DoDisconnect(HashableEdge e) {
            _perNodeEdges[e.a].Remove(e);
            _perNodeEdges[e.b].Remove(e);
            _edgeLookup.Remove(e.e);
            _fastEdgeSet.Remove(e);
            edges.Remove(e.e);
        }

        public E Connect(N a, N b) {
            if (AreConnected(a, b)) throw new GraphStructureException("Cannot connect, nodes already connected");
            if (!Contains(a) || !Contains(b)) throw new GraphStructureException("Both nodes must be in graph");            
            return _DoConnect(a,b);
        }

        public void Disconnect(N a, N b) {
            if (!Contains(a) || !Contains(b)) throw new GraphStructureException("Both nodes must be in graph");
            if (!AreConnected(a, b)) throw new GraphStructureException("Cannot disconnect, nodes not connected");
            _DoDisconnect((a,b));
        }

        public (bool success, E edge) TryConnect(N a, N b) {
            if (AreConnected(a, b)) return (false, default);
            if (!Contains(a) || !Contains(b)) return (false, default);
            return (true, _DoConnect(a, b));
        }

        public bool TryDisconnect(N a, N b) {
            if (!Contains(a) || !Contains(b)) return false;
            if (!AreConnected(a, b)) return false;
            _DoDisconnect((a,b));
            return true;
        }

        public bool AreConnected(N a, N b) => _fastEdgeSet.Contains(new HashableEdge(a, b));

        public E GetEdge(N a, N b) {
            _fastEdgeSet.TryGetValue((a, b), out var actualValue); // did not know HashSet implemented this! very useful!
            return actualValue.e;
        }

        public N GetOther(E edge, N node) {
            return _edgeLookup[edge].Other(node);
        }

        internal N GetEdgeNode(E e, int index) {
            if (index == 0) return _edgeLookup[e].a;
            else if (index == 1) return _edgeLookup[e].b;
            else throw new GraphStructureException("invalid index supplied");
        }

        struct HashableEdge {
            public readonly N a;
            public readonly N b;
            public readonly E e;
            public HashableEdge(N a, N b, E e = default) {
                if (a.GetHashCode() > b.GetHashCode()) { // ensure proper order
                    this.a = b; this.b = a; 
                } else {
                    this.a = a; this.b = b;
                }
                this.e = e; // note that you do not need to specify edge for most operations.
            }

            public bool Has(N node) => a.Equals(node) || b.Equals(node);

            public N Other(N node) {
                if (node.Equals(a)) return b;
                else if (node.Equals(b)) return a;
                else throw new GraphStructureException("You want other, but did not supply one of my two nodes");
            }

            public override bool Equals(object obj) { 
                return (obj is HashableEdge other) && other.a.Equals(a) && other.b.Equals(b);
            }

            public override int GetHashCode() => HashCode.Combine(a, b);

            public static implicit operator HashableEdge ((N a, N b) pair) => new(pair.a, pair.b);
        }
    }

    public class GraphStructureException : Exception {
        public GraphStructureException(string message) : base(message) { }
    }
}