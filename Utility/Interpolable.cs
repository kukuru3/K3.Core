using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace K3 {

    public delegate T InterpolationProcess<T>(T a, T b, float tFactor);

    public class BlendTree<T> where T: struct {

        T baseValue;

        public void SetBaseValue(T newBaseValue) {
            baseValue = newBaseValue;
        }

        struct BlendProcess {
            internal bool enabled;
            internal T target;
            internal float weight;
            internal float effectiveWeight;
            internal float currentBlendVelocity;
        }

        List<BlendProcess> activeProcesses = new List<BlendProcess>();

        public void SetBlendProcess(T target, float weight) {
            for (var i = 0; i < activeProcesses.Count; i++) { 
                if (activeProcesses[i].target.Equals(target)) {
                    var temp = activeProcesses[i];
                    temp.weight = weight;
                    activeProcesses[i] = temp;
                    return;
                }
            }
            activeProcesses.Add(new BlendProcess { target = target, weight = weight, enabled = true, effectiveWeight = float.Epsilon });
        }

        public void RemoveBlendProcess(T target) {
            
            for (var i = activeProcesses.Count-1; i >= 0; i--) if (activeProcesses[i].target.Equals(target)) {
                var temp = activeProcesses[i];
                temp.enabled = false;
                activeProcesses[i] = temp;
            }
        }

        readonly InterpolationProcess<T> interpolator;
        readonly float speed; 

        public BlendTree(InterpolationProcess<T> interpolator, float speed = 1f) {
            this.interpolator = interpolator;
            
        }

        public void Update(float deltaTime) {
            for (var i = 0; i < activeProcesses.Count; i++) {
                var p = activeProcesses[i];
                p.effectiveWeight = Mathf.SmoothDamp(p.effectiveWeight, p.enabled ? p.weight : 0f, ref p.currentBlendVelocity, 0.2f); // deltaTime * speed);
                activeProcesses[i] = p;
            }

            // prune:
            for (var i = activeProcesses.Count-1; i >= 0; i--) if ((!activeProcesses[i].enabled) && activeProcesses[i].effectiveWeight < float.Epsilon) {
                activeProcesses.RemoveAt(i);
            }
        }

        public T EffectiveValue { get {
            var effective = baseValue;
            foreach (var process in activeProcesses) {
                effective = interpolator(effective, process.target, process.effectiveWeight);
            }
            return effective;
        } }
    }

    public class Interpolation<T> {

        readonly InterpolationProcess<T> interpolator;
        readonly float speed;

        T source;
        T target;

        T effective;

        float tFactor;
        bool firstSet;
        bool hasTarget = false;

        public T EffectiveValue => effective;

        public Interpolation(InterpolationProcess<T> interpolator, float speed = 1f) {
            this.interpolator = interpolator;
            this.speed = speed;
            this.firstSet = true;
        }

        public void Update(float deltaTime) {
            if (hasTarget) { 
                var oldTFactor = tFactor;
                tFactor += deltaTime * speed;
                tFactor = UnityEngine.Mathf.Clamp01(tFactor);
                if (UnityEngine.Mathf.Approximately(oldTFactor, tFactor)) {
                    if (UnityEngine.Mathf.Approximately(tFactor, 1f)) {
                        source = target;
                        effective = target;
                        hasTarget = false;
                        return;
                    }
                }
                effective = interpolator(source, target, tFactor);
            }
        }

        public T Target { get => target; set {
            if (firstSet) {
                firstSet = false;
                source = value;
                effective = value;
                hasTarget = false;
                return;
            }
            source = effective;
            hasTarget = true;
            tFactor = 0f;
            target = value;
        } }
    }
}