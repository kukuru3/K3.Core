using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace K3.Collections {

    public class GraphOperations<N, E> {
        private readonly Graph<N, E> graph;

        public GraphOperations(Graph<N,E> graph) {
            this.graph = graph;
        }

        public HashSet<N> selection = new();

        public void Clear() {
            selection.Clear();
        }

        public ISet<N> FloodFill(IEnumerable<N> nodes, Predicate<N> spreadCriterion) {
            ISet<N> result = new HashSet<N>(nodes);
            int oldcount, newcount;
            do {
                oldcount = result.Count;
                result = Grow(result, spreadCriterion);
                newcount = result.Count;
            } while (newcount > oldcount);
            return result;
        }

        public ISet<N> FloodFill(IEnumerable<N> startingSet, Predicate<E> spreadCriterion) {
            ISet<N> result = new HashSet<N>(startingSet);
            int oldcount, newcount;
            do {
                oldcount = result.Count;
                result = Grow(result, spreadCriterion);
                newcount = result.Count;
            } while (newcount > oldcount);
            return result;
        }

        IEnumerable<N> Neighbours(N node) => graph.GetNeighbours(node);

        internal ISet<N> Grow(IEnumerable<N> startFrom, Predicate<N> spreadCriterion) {
            var result = new HashSet<N>();
            foreach (var node in startFrom) {
                result.Add(node);
                foreach (var n in Neighbours(node)) if (spreadCriterion(n)) result.Add(n);
            }
            return result;
        }

        // note: predicate by edge
        internal ISet<N> Grow(IEnumerable<N> startFrom, Predicate<E> spreadCriterion) {
            var result = new HashSet<N>();
            foreach (var node in startFrom) {
                result.Add(node);
                foreach ((var e, var n) in graph.GetEdges(node)) if (spreadCriterion(e)) result.Add(n);
            }
            return result;
        }

        Dictionary<N, DijkstraMetadata> dijkstraData = new();

        /// <summary>Get Dijkstra data of node "node"</summary>
        public DijkstraMetadata D(N node) {
            if (!dijkstraData.TryGetValue(node, out var data)) {
                data = new DijkstraMetadata();
                dijkstraData[node] = data;
            }
            return data;
        }

        public IList<ISet<N>> CalculateMasses(Predicate<E> edgeSpreadCriterion, Func<N, int> kernelPicker = null) {
            var remaining = new HashSet<N>(graph.AllNodes());
            var result = new List<ISet<N>>();

            int massID = 0;

            while (remaining.Count > 0) {
                massID++;
                var kernel = (kernelPicker != null) ? remaining.OrderBy(kernelPicker).First() : remaining.First();
                var filledSet = FloodFill(new HashSet<N>() { kernel }, edgeSpreadCriterion);
                foreach (var item in filledSet) {
                    D(item).regionID = massID;
                }
                result.Add(filledSet);
                remaining.ExceptWith(filledSet);
            }

            return result;
        }

        public void CalculateDijkstraDistances(Predicate<N> selectionCriterion) {
            var set = new HashSet<N>();
            foreach (var cell in graph.AllNodes()) { 
                D(cell).Reset();
                if (selectionCriterion(cell)) set.Add(cell);
            }
            CalculateDijkstraDistances(set);
        }

         public void CalculateDijkstraDistances(ISet<N> originalCells, int maxDistance = 999) {
            foreach (var cell in graph.AllNodes()) D(cell).Reset();

            var frontier = new HashSet<N>();
            var generation = 0;

            var closedSet = new HashSet<N>(originalCells);
            foreach (var cell in closedSet) { D(cell).status = Dijkstatus.Closed; D(cell).distance = 0; }

            do {
                generation++;
                frontier.Clear();
                foreach (var cell in closedSet) {
                    foreach (var n in Neighbours(cell)) { 
                        if (D(n).status == Dijkstatus.Closed) continue;
                        frontier.Add(n);
                        D(n).parent = cell;
                    }
                }
                foreach (var item in frontier) { 
                    D(item).distance = generation;
                    D(item).status = Dijkstatus.Closed;
                }

                closedSet.UnionWith(frontier);

            } while (frontier.Count > 0 && generation < maxDistance);
        }
        
        public enum Dijkstatus {
            None,
            Open,
            Closed,
        }
        public class DijkstraMetadata {
            internal Dijkstatus status;
            public N   parent;
            public int distance;
            public int regionID;

            internal void Reset() {
                distance = -1;
                regionID = 0;
                status = Dijkstatus.None;
                parent = default;
            }
        }
    }
}