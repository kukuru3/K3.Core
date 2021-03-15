using K3.Collections;

using System;
using System.Collections.Generic;
using System.Linq;

namespace K3.Modular {
    public class ModuleContainer : IAppModuleContainer {
        internal SimpleTree<IAppModule> ModuleTree { get; private set; }

        public event Action ModulesChanged;

        public void LaunchModule(IAppModule module, IAppModule parent = null) {
            if (ModuleTree == null) {
                if (parent != null) throw new System.ArgumentException("Would expect a non-parented state here");
                CreateStateTree(root: module);
            } else {
                if (parent == null) parent = ModuleTree.GetRoot();
                ModuleTree.Insert(module, parent);
            }
        }

        public void RemoveModule(IAppModule module) {
            ModuleTree.Remove(module);
        }

        public IEnumerable<IAppModule> AllModules => ModuleTree?.Iterate().Select(item => item.item) ?? new IAppModule[0];

        public IAppModule RootModule => ModuleTree.GetRoot();

        private void CreateStateTree(IAppModule root) {
            ModuleTree = new SimpleTree<IAppModule>(root);
            ModuleTree.ElementAdded += OnModuleAdded;
            ModuleTree.ElementRemoved += OnModuleRemoved;
            OnModuleAdded(root);
        }

        private void OnModuleAdded(IAppModule module) {
            module.OnWasInsertedIntoModuleContext();
            ModulesChanged?.Invoke();
        }

        private void OnModuleRemoved(IAppModule module) {
            module.OnWasRemovedFromModuleContext();
            ModulesChanged?.Invoke();
        }

        public T Get<T>() where T : IAppModule => ModuleTree.Iterate().Select(m => m.item).OfType<T>().FirstOrDefault();
    }
}