using System;
using System.Collections.Generic;
using System.Linq;

namespace K3.Collections {
    public class SimpleTree<T> where T : class {
        private TreeNode rootNode;

        private Dictionary<T, TreeNode> nodeLookup;

        public event Action<T> ElementAdded;
        public event Action<T> ElementRemoved;

        public SimpleTree(T root) {
            nodeLookup = new Dictionary<T, TreeNode>();
            CreateRoot(root);
        }

        public SimpleTree() {
            nodeLookup = new Dictionary<T, TreeNode>();
        }

        class TreeNode {
            public T Item { get; }

            public TreeNode(T item) {
                Item = item;
                children = new List<TreeNode>();
            }

            internal TreeNode parent;
            internal List<TreeNode> children;
        }

        public void CreateRoot(T rootItem) {
            if (rootNode != null) throw new InvalidOperationException("Root node already assigned");
            rootNode = new TreeNode(rootItem);
            nodeLookup.Add(rootItem, rootNode);
            ElementAdded?.Invoke(rootItem);
            InvalidateCache();
        }

        public void Insert(T item, T parent) {
            var parentNode = FindNodeOf(parent);
            if (parentNode == null) throw new ArgumentException($"Inserting {item} into tree under nonexistent parent {parent}");
            var currentNode = FindNodeOf(item);
            if (currentNode == null) currentNode = new TreeNode(item);

            Insert(currentNode, parentNode);
        }

        public IEnumerable<(T item, T parent)> Iterate() {
            foreach (var node in GetSubtree(rootNode))
                yield return (node.Item, node.parent?.Item);
        }

        public int CountChildren(T item) => FindNodeOf(item)?.children.Count ?? -1;
        public T GetParent(T item) => FindNodeOf(item).parent?.Item;

        public T GetRoot() => rootNode.Item; 

        public IEnumerable<T> GetChildren(T item) {
            var n = FindNodeOf(item);
            if (n != null) return n.children.Select(c => c.Item);
            else {
                UnityEngine.Debug.Log($"Item {item} has no associated node");
                return new T[0];
            }

        }

        /// <summary>Returns item, its parent, grandparent and so on all the way to the root.</summary>
        public IEnumerable<T> GetParentStack(T item) {
            var node = FindNodeOf(item);
            if (node == null) throw new ArgumentException($"Cannot get parent stack of {item} since item is not in the tree");
            while (node != null) {
                yield return node.Item;
                node = node.parent;
            }
        }

        public void Remove(T item) {
            var node = FindNodeOf(item);
            var subtree = GetSubtree(node);
            foreach (var nn in subtree.Reverse()) Remove(nn);
            InvalidateCache();
        }

        public IEnumerable<T> GetFlatSubtree(T fromItem) {
            var node = FindNodeOf(fromItem);
            var subtree = GetSubtree(node);
            foreach (var nn in subtree) yield return nn.Item;
        }

        private TreeNode FindNodeOf(T item) {
            nodeLookup.TryGetValue(item, out var node);
            return node;
        }

        private void Insert(TreeNode node, TreeNode parent) {
            if (node.parent == parent) return;
            if (node.parent != null) Unparent(node);

            node.parent = parent;
            parent.children.Add(node);

            nodeLookup.Add(node.Item, node);
            ElementAdded?.Invoke(node.Item);
            InvalidateCache();
        }

        private void Remove(TreeNode node) {
            Unparent(node);
            nodeLookup.Remove(node.Item);
            ElementRemoved?.Invoke(node.Item);
            InvalidateCache();
        }

        private void Unparent(TreeNode node) {
            var didRemove = node.parent?.children.Remove(node) ?? true;
            if (!didRemove) throw new InvalidOperationException("Unparenting node that did not exist in children collection");
            node.parent = null;
        }

        IEnumerable<TreeNode> GetSubtree(TreeNode fromNode) {
            yield return fromNode;
            foreach (var child in fromNode.children) foreach (var subtreeItem in GetSubtree(child)) yield return subtreeItem;
        }

        List<T> compiledList = null;

        void InvalidateCache() => compiledList = null;

        void RegenerateCompiledList() { compiledList = new List<T>(); foreach (var q in Iterate()) compiledList.Add(q.item); }

        public IEnumerable<T> FlatListOfItems { get {
            if (compiledList == null) 
                RegenerateCompiledList();
            return compiledList;
        } }
    }
}
