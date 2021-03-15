using System;
using System.Collections.Generic;

namespace K3.UI {
    public enum UIEffectorTypes {
        Trigger,
        BoolValue,
        DiscreteValue, // integer
        ContinousValue, // float
    }

    public interface IUIElement {
        UIEffectorTypes EffectorType { get; }
    }

    public interface ITriggerElement : IUIElement {
        event Action Triggered;
    }

    public interface IValueElement<TValue> : IUIElement {
        event Action ValueUpdated;
        TValue Value { get; }
    }

    public class UIActionDefinition {
        public UIEffectorTypes type;
        public string id;
        public Delegate cachedDelegate;
    }

    public interface IUIController {
        IEnumerable<UIActionDefinition> ListActions(UIEffectorTypes forType);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UILogicAttribute : System.Attribute {
        public string ID { get; }
        public UILogicAttribute(string id) {
            ID = id;
        }
    }

    public delegate void TriggerDelegate();
    public delegate void OnOffDelegate(bool onOff);
    public delegate void IntValueDelegate(int value);
    public delegate void FloatValueDelegate(float value);
}
