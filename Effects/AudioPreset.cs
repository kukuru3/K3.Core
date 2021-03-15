﻿using UnityEngine;

namespace K3.Effects {
    [CreateAssetMenu(fileName ="audio.preset.asset", menuName = "K3 Gamepak/Audio propagation preset")]
    public class AudioPreset : Resource {
        #pragma warning disable 649
        /// <summary>
        /// An exponential number. 0 is inaudible, 1 is whisper, 10 is mother of all nuclear blasts.
        /// </summary>
        [SerializeField][Range(0f, 1f)] float strengthFactor; // 0 is inaudible, 1 is whisper, 10 is nuke
        [SerializeField] bool linearRolloff;
        [SerializeField][Range(0f, 1f)]float dopplerLevel = 1f;
        [SerializeField][Range(0f, 1f)]float ambient = 1f;
        [SerializeField] UnityEngine.Audio.AudioMixerGroup mixerGroup;

        #pragma warning restore 649
        public void Apply(AudioSource source) {

            if (ambient < 0.5f) {
                source.spatialBlend = 1f; // 3d
                source.spread = ambient * 360f;
            } else {
                source.spatialBlend = 1f - ambient * 2f;
                source.spread = 180f;
            }

            // source.spatialBlend = 1f - Mathf.Clamp01(Mathf.Abs(Mathf.Pow(ambient - 0.5f, 3)));
            source.rolloffMode = linearRolloff ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic;
            var D = Mathf.Pow((float)System.Math.E, strengthFactor * 10f) * 0.1F - 0.1F;
            if (source.rolloffMode == AudioRolloffMode.Logarithmic) {
                source.minDistance = D;
                source.maxDistance = D * 30;
            } else {
                source.minDistance = D / 2;
                source.maxDistance = D * 5; // numbers completely arbitrary!
            }
            source.dopplerLevel = dopplerLevel;
            source.spread = Mathf.Sqrt(ambient) * 180f;
            source.outputAudioMixerGroup = mixerGroup;
        }
    }
}
