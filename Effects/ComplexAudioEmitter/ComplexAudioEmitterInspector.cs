#if UNITY_EDITOR

using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace K3.Effects.Editor {
    [CustomEditor(typeof(ComplexAudioEmitter))]
    class ComplexAudioEmitterInspector : UnityEditor.Editor {

        private ReorderableList feedsList;
        private ReorderableList effectsList;

        ComplexAudioEmitter TargetEmitter => (ComplexAudioEmitter)serializedObject.targetObject;

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("masterVolume"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("preset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("channels"), true);

            EditorGUILayout.Separator();

            feedsList.DoLayoutList();

            EditorGUILayout.Separator();

            effectsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        int DrawChannelSelector(Rect sourceRect, int sourceValue) {
            var channelNames = TargetEmitter.Channels.Select(c => c.Name).ToArray();
            return EditorGUI.Popup(sourceRect, sourceValue, channelNames);
        }

        int DrawFeedSelector(Rect sourceRect, int sourceValue) {
            return EditorGUI.Popup(sourceRect, sourceValue, TargetEmitter.FeedNames.ToArray());
        }

        ComplexAudioEmitter.FeedProcessing DrawEffectSelector(Rect rect, ComplexAudioEmitter.FeedProcessing processing) {
            return (ComplexAudioEmitter.FeedProcessing)EditorGUI.EnumPopup(rect, processing);
        }

        public void OnEnable() {
            feedsList = new ReorderableList(serializedObject, serializedObject.FindProperty("feeds"), false, true, !Application.isPlaying, !Application.isPlaying);
            effectsList = new ReorderableList(serializedObject, serializedObject.FindProperty("influences"), true, true, true, true);

            feedsList.drawElementCallback = DrawFeedElement;
            effectsList.drawElementCallback = DrawEffectElement;

            feedsList.drawHeaderCallback = r => EditorGUI.LabelField(r, "Input feeds");
            effectsList.drawHeaderCallback = r => EditorGUI.LabelField(r, "Effects");

            void DrawFeedElement(Rect rect, int index, bool isActive, bool isFocused) {
                var element = feedsList.serializedProperty.GetArrayElementAtIndex(index);
                var name = element.stringValue;
                name = EditorGUI.TextField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), name);
                element.stringValue = name;

                if (Application.isPlaying) {
                    var v = TargetEmitter.GetFeed(index);
                    v = EditorGUI.Slider(new Rect(rect.x + 110, rect.y, 140, EditorGUIUtility.singleLineHeight), v, 0f, 1f);
                    TargetEmitter.SetFeed(index, v);
                }
            }

            void DrawEffectElement(Rect rect, int index, bool isActive, bool isFocused) {
                //// Since unity is a bit... special... we can't just say effectsList.SerializedProperty.GetArrayElementAtIndex(index).objectValue - because that would actually make sense
                var influencesArray = ((System.Collections.IList)typeof(ComplexAudioEmitter).GetField("influences", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(TargetEmitter));
                var obj = influencesArray[index] as ComplexAudioEmitter.FeedInfluence;

                var flags = BindingFlags.NonPublic | BindingFlags.Instance;

                var w = (int)(EditorGUIUtility.currentViewWidth - rect.x * 2);

                var targetFeed = (int)obj.GetType().GetField("feedIndex", flags).GetValue(obj);
                var targetChannel = (int)obj.GetType().GetField("targetChannel", flags).GetValue(obj);
                var targetEffect = (ComplexAudioEmitter.FeedProcessing)obj.GetType().GetField("effect", flags).GetValue(obj);
                var targetCurve = (AnimationCurve)obj.GetType().GetField("curve", flags).GetValue(obj);

                var h = EditorGUIUtility.singleLineHeight;

                Rect ControlRect(float startNormalized, float widthNormalized) => new Rect(rect.x + (w * startNormalized), rect.y, w * widthNormalized, h);

                targetFeed = DrawFeedSelector(ControlRect(0f, 0.25f), targetFeed);
                targetChannel = DrawChannelSelector(ControlRect(0.25f, 0.25f), targetChannel);
                targetEffect = DrawEffectSelector(ControlRect(0.5f, 0.15f), targetEffect);
                targetCurve = EditorGUI.CurveField(ControlRect(0.65f, 0.35f), targetCurve);

                obj.GetType().GetField("feedIndex", flags).SetValue(obj, targetFeed);
                obj.GetType().GetField("targetChannel", flags).SetValue(obj, targetChannel);
                obj.GetType().GetField("effect", flags).SetValue(obj, targetEffect);
                obj.GetType().GetField("curve", flags).SetValue(obj, targetCurve);
            }
        }
    }
}

#endif