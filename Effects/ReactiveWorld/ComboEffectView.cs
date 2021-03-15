﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace K3.ReactiveWorld {

    // Let's say you want to shoot a cannon. Obviously, some effects happen. Audio is one. Particles are
    // another. There may be a flash of light. Some geometry can temporarily appear.

    // The cannon shell, as it travels in the game world, emits audio, that is doplerised. It also raises
    // dust off the ground to the left and right of the shell trail. Its passing close to the camera might
    // cause it to shake and emit a rumbling sound. 

    // When the shell collides with the geometry, something happens. If it hits the ground, a bunch of ground
    // particles might happen, and a fine dust will fall afterwards. Or, if it impacts water, a splash of water,
    // followed by water droplets.

    // Normally, you might implement all this as having a parent GameObject which has a bunch of sub-objects, 
    // and then each of them does its own thing. You can have "spawn this when it impacts water" and "spawn that
    // when it impacts ground". But what if tomorrow you want to add snow and ice, and have other effects there?

    // Webster's dictionary defines a "combo effect" as an "ingenious thing KUKURU3 came up to be able to 
    // easily define complex combinations of sound emitters, particles and other things"

    // INTENDED USES FOR COMBO EFFECTS:
    // - bullet impact raises smoke particles, has an audio and possibly a flashing light
    // - rocket in flight has "whoosh" audio source, a light, and a particle trail. When the rocket hits anything, its effect view
    //   is detached and phases out (stopping particle emission, and living until the light phases out and there are no more particles

    public class ComboEffectView : Script {

        ParticleSystem[] particles;
        AudioSource[] sources;
        Light[] lights;

        private Transform followsObject;
        bool wasAttached = false;
        Vector3 relativePosition;
        Quaternion relativeRotation;

        float deathProcessFinalMultiplier = 1f;
        bool deathProcess = false;
        bool sanctionedDestruction;
        float lastNormalizedTimeApplied;

        float life;

        List<SubEffect> autogeneratedEffects = new List<SubEffect>();

        #pragma warning disable 649
        [SerializeField] bool manuallyControlled; // if true, the strength of the effect is something you set yourself.
        [SerializeField] float duration; // all curves and effects are normalized in relation to this
        [SerializeField] float randomizeDuration;
        [SerializeField] SubEffect[] subEffects;
        [SerializeField] float phaseOutDuration;
        #pragma warning restore 649

        enum EffectProcessing {
            ParticleSpawnMultiplier,
            LightLevel,
            AudioPitch,
            AudioVolume,
        }

        [Serializable] 
        class SubEffect {
            #pragma warning disable 649
            [SerializeField] internal GameObject target;
            [SerializeField] internal EffectProcessing effect;
            [SerializeField] AnimationCurve curve;
            [SerializeField] internal float deviation;
            #pragma warning restore 649

            internal bool autoGenerated;
            internal ParticleSystem cachedParticles;
            internal Light cachedLight;
            internal AudioSource cachedAudioSource;

            internal float initialIntensity; // initial value of a light source intensity, or audio source volume.

            internal float finalDeviation; // a "collapsed" deviation, randomly selected.

            internal float EvaluateCurve(float normalizedTime) {
                if (autoGenerated) return 1f;
                var value = curve.Evaluate(normalizedTime);
                if (deviation > float.Epsilon) value = Randoms.Deviate(value, finalDeviation);
                return value;
            }
        }

        // a combo effect can control / animate particle things like particle dispersal

        Dictionary<Component, float> cachedValues = new Dictionary<Component, float>();

        protected override void Init() { 
            sources = GetComponentsInChildren<AudioSource>();
            particles = GetComponentsInChildren<ParticleSystem>();
            lights = GetComponentsInChildren<Light>();

            // for every component that is unhandled by subeffects:

            var unhandledComponents= new HashSet<Component>();
            unhandledComponents.UnionWith(sources);
            unhandledComponents.UnionWith(particles);
            unhandledComponents.UnionWith(lights);

            if (!manuallyControlled && duration < float.Epsilon) throw new ArgumentException("Duration must be > 0 for non-manually controlled effects");

            foreach (var infl in subEffects) {
                CacheComponents(infl);
                infl.finalDeviation = UnityEngine.Random.Range(-1f, 1f) * infl.deviation;
                unhandledComponents.Remove(infl.cachedAudioSource);
                unhandledComponents.Remove(infl.cachedLight);
                unhandledComponents.Remove(infl.cachedParticles);
            }

            foreach (var unhandledComponent in unhandledComponents) {
                var eff = AutogenerateEffectFor(unhandledComponent);
                autogeneratedEffects.Add(eff);
                CacheComponents(eff);
            }

            if (manuallyControlled) Apply(0f); // start at full - dangerous assumption to always make perhaps?
            if (transform.parent != null) {
                LooselyAttachToObject(transform.parent, true);
                transform.parent = null;
            }
        }

        SubEffect AutogenerateEffectFor(Component c) {
            var se = new SubEffect() {
                autoGenerated = true,
                target = c.gameObject,
                deviation = 0,
            };
            if (c is Light l) se.effect = EffectProcessing.LightLevel;
            else if (c is ParticleSystem ps) se.effect = EffectProcessing.ParticleSpawnMultiplier;
            else if (c is AudioSource a) 
                se.effect = EffectProcessing.AudioVolume;
            else
                throw new NotImplementedException("Cannot resolve default effector");
            return se;
        }

        protected override void Teardown(bool becauseOfExitingApp) {
            // check for abrupt death
            if (!becauseOfExitingApp && !sanctionedDestruction) Debug.Log($"{name} being destroyed without phasing out.");
        }

        public void LooselyAttachToObject(Transform newParent, bool preserveRelativePosition) {
            wasAttached = true;
            followsObject = newParent;
            if (preserveRelativePosition) {
                relativePosition = newParent.InverseTransformPoint(transform.position);
                relativeRotation = Quaternion.Inverse(newParent.rotation) * transform.rotation;
            } else {
                relativePosition = Vector3.zero;
                relativeRotation = Quaternion.identity;
            }
            
        }

        /// <summary> Use this if you want to manually control the "value" of the emitter.</summary>
        public void SetValue(float val) {
            if (!manuallyControlled) throw new ArgumentException("Can only set strength of manually controlled effects.");
            Apply(val);
        }

        private void Apply(float normalizedTime) {
            lastNormalizedTimeApplied = normalizedTime;
            foreach (var infl in subEffects) ApplyInfluence(infl, normalizedTime);
            foreach (var eff in autogeneratedEffects) ApplyInfluence(eff, normalizedTime);
        }

        private void LateUpdate() {
            if (followsObject != null) {
                transform.position = followsObject.TransformPoint(relativePosition);
                transform.rotation = followsObject.rotation * relativeRotation;
            }

            if (wasAttached && followsObject == null) {
                StartDeathProcess();
            }
        }

        private void Update() {
            life += Time.deltaTime;
            
            if (deathProcess) {
                deathProcessFinalMultiplier -= Time.deltaTime / phaseOutDuration;
                if (deathProcessFinalMultiplier < 0f) {
                    deathProcessFinalMultiplier = 0f;
                }
                if (manuallyControlled) { Apply(lastNormalizedTimeApplied); } // because it woulldn't get applied otherwise
            }

            if (!manuallyControlled) {
                Apply(life / duration);
                if (life >= duration) StartDeathProcess();
            }
        }

        private void StartDeathProcess() {
            this.deathProcess = true;
        }


        private bool ComponentIdle(Component c) {
            if (c == null) return true;
            if (c is Light l) return !(l.enabled && l.intensity > float.Epsilon);
            else if (c is ParticleSystem ps) return ps.particleCount == 0;
            else if (c is AudioSource @as) return !@as.isPlaying || @as.volume < float.Epsilon;
            else return true;
        }

        protected internal override void Logic() {
            // if manual control, no sources are audible, and no particles are emitting, and no lights are glowing, destroy this.
            if (manuallyControlled || deathProcess) {
                // note: if you have lights and audios that are unmanaged by curves
                bool allIdle = particles.All(p => ComponentIdle(p))
                            && sources.All(s => ComponentIdle(s))
                            && lights.All(l => ComponentIdle(l));

                if (allIdle) {
                    sanctionedDestruction = true;
                    Destroy(gameObject);
                }
            }
            // this should probably be controlled via a constant, and have an opt-in "eternal" boolean to prohibit it from being shown.
            // if (manuallyControlled && Mathf.FloorToInt(life) == 30) Debug.Log($"Effect {name} has been alive for 30 seconds now; is this intentional?");
        }
        
        #region Guts of the class, working with influences.
        private void ApplyInfluence(SubEffect subEffect, float normalizedTime) {
            var value = subEffect.EvaluateCurve(normalizedTime);

            switch (subEffect.effect) {
                case EffectProcessing.ParticleSpawnMultiplier:
                    var em = subEffect.cachedParticles.emission;
                    em.rateOverTimeMultiplier = value * deathProcessFinalMultiplier;
                    em.rateOverDistanceMultiplier = value * deathProcessFinalMultiplier;
                    break;
                case EffectProcessing.LightLevel:
                    subEffect.cachedLight.intensity = subEffect.initialIntensity * value * deathProcessFinalMultiplier;
                    break;
                case EffectProcessing.AudioVolume:
                    subEffect.cachedAudioSource.volume = subEffect.initialIntensity * value * deathProcessFinalMultiplier;
                    break;
                case EffectProcessing.AudioPitch:
                    subEffect.cachedAudioSource.pitch = subEffect.initialIntensity * value;
                    break;
                default:
                    throw new NotImplementedException($"Unhandled influence type : {subEffect.effect}");
            }
        }

        void ValidateComponent(SubEffect i, Component c) {
            if (c == null) throw new ArgumentException($"Unknown component : influence {i.effect} targeting {i.target}");
        }

        private void CacheComponents(SubEffect infl) {
            infl.cachedParticles = infl.target.GetComponent<ParticleSystem>();
            infl.cachedLight = infl.target.GetComponent<Light>();
            infl.cachedAudioSource = infl.target.GetComponent<AudioSource>();

            switch (infl.effect) {
                case EffectProcessing.ParticleSpawnMultiplier:
                    ValidateComponent(infl, infl.cachedParticles);
                    break;
                case EffectProcessing.LightLevel:
                    ValidateComponent(infl, infl.cachedLight);
                    infl.initialIntensity = infl.cachedLight.intensity;
                    break;
                case EffectProcessing.AudioVolume:
                    ValidateComponent(infl, infl.cachedAudioSource);
                    infl.initialIntensity = infl.cachedAudioSource.volume;
                    break;
                case EffectProcessing.AudioPitch:
                    ValidateComponent(infl, infl.cachedAudioSource);
                    infl.initialIntensity = infl.cachedAudioSource.pitch;
                    break;
                default:
                    throw new NotImplementedException("Unhandled effect processing");
            }
        } 
        #endregion
    }
}
