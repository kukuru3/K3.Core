using System;
using System.Collections.Generic;

using UnityEngine;

namespace K3.UI {

    public class UIControllerFunctionality {
        Dictionary<string, UIActionDefinition> actionCache = null;
        readonly MonoBehaviour sourceComponent;
        public UIControllerFunctionality(MonoBehaviour source) {
            this.sourceComponent = source;
            InitializeChildren();
        }

        private void InitializeChildren() {
            var allChildren = sourceComponent.gameObject.GetComponentsInChildren<UILogic>(true);
            foreach (var logic in allChildren) {
                var capturedLogic = logic;

                var element = logic.GetComponent<IUIElement>();

                if (element == null)
                    throw new Exception($"{nameof(UILogic)} must have a {nameof(IUIElement)} attached to it");

                if (element is ITriggerElement trigger) {
                    trigger.Triggered += () => HandleTriggered(capturedLogic.MessageID);
                } else if (element is IValueElement<int> valueControl) {
                    valueControl.ValueUpdated += () => HandleIntValueUpdated(capturedLogic.MessageID, valueControl.Value);
                } else if (element is IValueElement<bool> valueControlBool) {
                    valueControlBool.ValueUpdated += () => HandleToggleUpdated(capturedLogic.MessageID, valueControlBool.Value);
                } else if (element is IValueElement<float> valueControlFloat) {
                    valueControlFloat.ValueUpdated += () => HandleFloatValueUpdated(capturedLogic.MessageID, valueControlFloat.Value);
                } else {
                    throw new Exception($"Unhandled type of element : {capturedLogic}");
                }
            }
        }

        private void HandleTriggered(string actionID) {
            if (actionCache == null) BuildActionCache();
            if (!actionCache.TryGetValue(actionID, out var res)) return;
            ((TriggerDelegate)res.cachedDelegate)?.Invoke();
        }

        private void HandleButtonClicked(string id) {
            if (actionCache == null) BuildActionCache();
            if (!actionCache.TryGetValue(id, out var res)) return;
            ((TriggerDelegate)res.cachedDelegate)?.Invoke();
        }

        private void HandleToggleUpdated(string id, bool value) {
            if (actionCache == null) BuildActionCache();
            if (!actionCache.TryGetValue(id, out var res)) return;
            ((OnOffDelegate)res.cachedDelegate)?.Invoke(value);
        }

        private void HandleIntValueUpdated(string id, int value) {
            if (actionCache == null) BuildActionCache();
            if (!actionCache.TryGetValue(id, out var res)) return;
            ((IntValueDelegate)res.cachedDelegate)?.Invoke(value);
        }

        private void HandleFloatValueUpdated(string id, float value) {
            if (actionCache == null) BuildActionCache();
            if (!actionCache.TryGetValue(id, out var res)) return;
            ((FloatValueDelegate)res.cachedDelegate)?.Invoke(value);
        }

        private void BuildActionCache() {
            var attribs = K3.ReflectionUtility.ListMethodAttributes<UILogicAttribute>(sourceComponent.GetType());
            actionCache = new Dictionary<string, UIActionDefinition>();
            foreach (var (attr, method) in attribs) {

                var effKind = FindEffectorTypeFor(method);
                Delegate d = null;
                Type t;
                switch (effKind) {
                    case UIEffectorTypes.Trigger: t = typeof(TriggerDelegate); break;
                    case UIEffectorTypes.BoolValue: t = typeof(OnOffDelegate); break;
                    case UIEffectorTypes.DiscreteValue: t = typeof(IntValueDelegate); break;
                    case UIEffectorTypes.ContinousValue: t = typeof(FloatValueDelegate); break;
                    default:
                        throw new System.Exception("Unhandled effector type");
                }

                if (t != null) {
                    try {
                        d = Delegate.CreateDelegate(t, sourceComponent, method.Name);
                    } catch (ArgumentException e) {
                        Debug.LogError($"Bind attempt failed trying to bind method {method} to a delegate of type {t} in {this.sourceComponent}");
                    }

                    if (d != null) {
                        actionCache.Add(attr.ID, new UIActionDefinition {
                            cachedDelegate = d,
                            id = attr.ID,
                            type = effKind,
                        });
                    }

                } else {
                    throw new System.Exception($"Didn't find suitable delegate for method attr {attr.ID} on method `{method.Name}`");
                }
            }
        }

        UIEffectorTypes FindEffectorTypeFor(System.Reflection.MethodInfo method) {
            var paramInfos = method.GetParameters();
            if (paramInfos.Length == 0) return UIEffectorTypes.Trigger;
            else if (paramInfos.Length == 1) {
                if (paramInfos[0].ParameterType == typeof(int)) return UIEffectorTypes.DiscreteValue;
                else if (paramInfos[0].ParameterType == typeof(bool)) return UIEffectorTypes.BoolValue;
                else if (paramInfos[0].ParameterType == typeof(int)) return UIEffectorTypes.DiscreteValue;
                else if (paramInfos[0].ParameterType == typeof(float)) return UIEffectorTypes.ContinousValue;
            }
            throw new Exception($"Could not infer effector type for {method.DeclaringType}::{method.Name}");
        }

        public IEnumerable<UIActionDefinition> ListActions(UIEffectorTypes type) {
            if (actionCache == null) BuildActionCache();
            foreach (var action in actionCache) if (action.Value.type == type) yield return action.Value;
        }
    }

    //public abstract class BaseUIController : K3.Script, IUIController {

    //    Dictionary<string, UIActionDefinition> actionCache = null;

    //    protected override void Init() {
    //        var allChildren = GetComponentsInChildren<UILogic>(true);
    //        foreach (var logic in allChildren) {
    //            var capturedLogic = logic;

    //            var element = logic.GetComponent<IUIElement>();

    //            if (element == null)
    //                throw new Exception($"{nameof(UILogic)} must have a {nameof(IUIElement)} attached to it");

    //            if (element is ITriggerElement trigger) {
    //                trigger.Triggered += () => HandleTriggered(capturedLogic.MessageID);
    //            } else if (element is IValueElement<int> valueControl) {
    //                valueControl.ValueUpdated += () => HandleIntValueUpdated(capturedLogic.MessageID, valueControl.Value);
    //            } else if (element is IValueElement<bool> valueControlBool) {
    //                valueControlBool.ValueUpdated += () => HandleToggleUpdated(capturedLogic.MessageID, valueControlBool.Value);
    //            } else if (element is IValueElement<float> valueControlFloat) {
    //                valueControlFloat.ValueUpdated += () => HandleFloatValueUpdated(capturedLogic.MessageID, valueControlFloat.Value);
    //            } else {
    //                throw new Exception($"Unhandled type of element : {capturedLogic}");
    //            }
    //        }
    //        // find children, subscribe to delegates
    //    }

    //    private void HandleTriggered(string actionID) {
    //        if (actionCache == null) BuildActionCache();
    //        if (!actionCache.TryGetValue(actionID, out var res)) return;
    //        ((TriggerDelegate)res.cachedDelegate)?.Invoke();
    //    }

    //    private void HandleButtonClicked(string id) {
    //        if (actionCache == null) BuildActionCache();
    //        if (!actionCache.TryGetValue(id, out var res)) return;
    //        ((TriggerDelegate)res.cachedDelegate)?.Invoke();
    //    }

    //    private void HandleToggleUpdated(string id, bool value) {
    //        if (actionCache == null) BuildActionCache();
    //        if (!actionCache.TryGetValue(id, out var res)) return;
    //        ((OnOffDelegate)res.cachedDelegate)?.Invoke(value);
    //    }

    //    private void HandleIntValueUpdated(string id, int value) {
    //        if (actionCache == null) BuildActionCache();
    //        if (!actionCache.TryGetValue(id, out var res)) return;
    //        ((IntValueDelegate)res.cachedDelegate)?.Invoke(value);
    //    }

    //    private void HandleFloatValueUpdated(string id, float value) {
    //        if (actionCache == null) BuildActionCache();
    //        if (!actionCache.TryGetValue(id, out var res)) return;
    //        ((FloatValueDelegate)res.cachedDelegate)?.Invoke(value);
    //    }

    //    IEnumerable<UIActionDefinition> IUIController.ListActions(UIEffectorTypes type) {
    //        if (actionCache == null) BuildActionCache();
    //        foreach (var action in actionCache) if (action.Value.type == type) yield return action.Value;
    //    }

    //    private void BuildActionCache() {
    //        var attribs = K3.ReflectionUtility.ListMethodAttributes<UILogicAttribute>(GetType());
    //        actionCache = new Dictionary<string, UIActionDefinition>();
    //        foreach (var (attr,method) in attribs) {

    //            var effKind = FindEffectorTypeFor(method);
    //            Delegate d;
    //            Type t;
    //            switch (effKind) {
    //                case UIEffectorTypes.Trigger: t = typeof(TriggerDelegate); break;
    //                case UIEffectorTypes.BoolValue: t = typeof(OnOffDelegate); break;
    //                case UIEffectorTypes.DiscreteValue: t = typeof(IntValueDelegate); break;
    //                case UIEffectorTypes.ContinousValue: t = typeof(FloatValueDelegate); break;
    //                default:
    //                    throw new System.Exception("Unhandled effector type");
    //            }

    //            if (t != null) {
    //                d = Delegate.CreateDelegate(t, this, method.Name);
    //                actionCache.Add(attr.ID, new UIActionDefinition {
    //                    cachedDelegate = d,
    //                    id = attr.ID,
    //                    type = effKind,
    //                });
    //            } else {
    //                throw new System.Exception($"Didn't find suitable delegate for method attr {attr.ID} on method `{method.Name}`");
    //            }
    //        }
    //    }

    //    UIEffectorTypes FindEffectorTypeFor(System.Reflection.MethodInfo method) {
    //        var paramInfos = method.GetParameters();
    //        if (paramInfos.Length == 0) return UIEffectorTypes.Trigger;
    //        else if (paramInfos.Length == 1) {
    //            if (paramInfos[0].ParameterType == typeof(int)) return UIEffectorTypes.DiscreteValue;
    //            else if (paramInfos[0].ParameterType == typeof(bool)) return UIEffectorTypes.BoolValue;
    //            else if (paramInfos[0].ParameterType == typeof(int)) return UIEffectorTypes.DiscreteValue;
    //            else if (paramInfos[0].ParameterType == typeof(float)) return UIEffectorTypes.ContinousValue;
    //        }
    //        throw new Exception($"Could not infer effector type for {method.DeclaringType}::{method.Name}");
    //    }


    //}
}
