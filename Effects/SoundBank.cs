using System.Collections.Generic;

using UnityEngine;

namespace K3.Effects {
    [CreateAssetMenu(fileName = "soundbank.asset", menuName = "K3 Gamepak/Sound bank")]
    public class SoundBank : Resource {
        #pragma warning disable 649
        [SerializeField] bool looping;
        [SerializeField] AudioClip[] clips;
        [SerializeField] AudioPreset propagation;

        [SerializeField] float minPitch = 1f;
        [SerializeField] float maxPitch = 1f; 

        [SerializeField] float minVolume = 1f;
        [SerializeField] float maxVolume = 1f;

        // todo later: consider making pitch & volume variations per-clip 
        #pragma warning restore 649

        bool UseShuffle => clips.Length > 5;
        Randoms.ShuffledArrayView<AudioClip> shuffler;

        public int ClipCount => clips.Length;
        public AudioClip GetClip(int index) => clips[index];

        public AudioClip GetNextClip() {
            if (clips.Length == 0) return null;
            if (UseShuffle) {
                if (shuffler == null) InitializeShuffler();
                return shuffler.NextValue();
            }
            return clips.PickRandom();
        }

        private void InitializeShuffler() {
            shuffler = new Randoms.ShuffledArrayView<AudioClip>(clips);
        }

        public AudioPreset Propagation => propagation;
        public bool Looping => looping;

        public float Pitch => Random.Range(minPitch, maxPitch);
        public float Volume { get { var v = Random.Range(minVolume, maxVolume); return v * v; } }
    }
}
#if UNITY_EDITOR 
namespace K3.Editor {
    using K3.Effects;
    using UnityEditor;
    [CustomEditor(typeof(SoundBank)), CanEditMultipleObjects]
    public class SoundBankInspector : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clips"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("propagation"));

            var minPitchProp = serializedObject.FindProperty("minPitch");
            var maxPitchProp = serializedObject.FindProperty("maxPitch");
            var minVolumeProp = serializedObject.FindProperty("minVolume");
            var maxVolumeProp = serializedObject.FindProperty("maxVolume");
            var minPitch = minPitchProp.floatValue;
            var maxPitch = maxPitchProp.floatValue;
            var minVol = minVolumeProp.floatValue;
            var maxVol = maxVolumeProp.floatValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Volume:", GUILayout.MaxWidth(60f));
            EditorGUILayout.MinMaxSlider(ref minVol, ref maxVol, 0.1f, 1f);
            EditorGUILayout.LabelField($"{minVol:F} - {maxVol:F}", GUILayout.MaxWidth(80f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pitch:", GUILayout.MaxWidth(60f));
            EditorGUILayout.MinMaxSlider(ref minPitch, ref maxPitch, 0.3f, 2f);
            EditorGUILayout.LabelField($"{minPitch:F} - {maxPitch:F}", GUILayout.MaxWidth(80f));
            EditorGUILayout.EndHorizontal();

            minPitchProp.floatValue  = minPitch;
            maxPitchProp.floatValue  = maxPitch;
            minVolumeProp.floatValue = minVol;
            maxVolumeProp.floatValue = maxVol;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("looping"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif