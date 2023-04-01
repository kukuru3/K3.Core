using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace K3.Effects {
    /// <summary>
    /// Declares "channels", "feeds" and "effects".
    /// Each channel will spawn an audio source at runtime which will loop an audio clip. 
    /// "Effects" will influence pitch and volume of individual channels via animation curves
    /// The evaluation of animation curve is done through values called "feeds"
    /// You can declare any number of input feeds via a string. 
    /// Input feed values will default to 0.0 and you are expected to give them values at runtime.
    /// A feed is accessible with either a string name or integer index.
    /// </summary>
    public partial class ComplexAudioEmitter : MonoBehaviour, IFeedReceiver, IChannelMaster {
        #pragma warning disable 649
        [SerializeField] Channel[] channels;
        [SerializeField] string[] feeds;
        [SerializeField] FeedInfluence[] influences;
        [SerializeField] [Range(0f, 1f)] float masterVolume = 1f;
        [SerializeField] AudioPreset preset;
        #pragma warning restore 649
        public IEnumerable<Channel> Channels => channels;

        public IEnumerable<string> FeedNames => feeds;

        bool dirty = true;
        Dictionary<string, int> feedIDLookup;
        float[] feedValues;

        #region Public interface

        public float MasterVolume {
            get { return masterVolume; }
            set { masterVolume = value; dirty = true; }
        }

        public void Stop() {
            MasterVolume = 0f;
            dirty = true;
        }

        public void Play(float atMasterVolume = 1f) {
            MasterVolume = atMasterVolume;
            foreach (var c in channels) c.StartPlayingAudio();
        }

        public float GetFeed(int feed) => _SafeGetFeedValue(feed);

        public float GetFeed(string feed) => _SafeGetFeedValue(LookupFeedIndex(feed));

        public void SetFeed(int feedIndex, float value) => _SafeSetFeedValue(feedIndex, value);

        public void SetFeed(string feed, float value) => _SafeSetFeedValue(LookupFeedIndex(feed), value);
        #endregion

        // Since in many cases you will have just one feed, it makes sense to just have a single quick property to access it. 
        // similar to how Unity handles MeshRenderer.material as a shortcut to SetMaterial(0) and GetMaterial(0)
        public float FeedValue {
            get => GetFeed(0);
            set => SetFeed(0, value);
        }

        protected void Start() {            
            if (feeds == null || feeds.Length == 0) feeds = new[] { "Value" };
            CreateFeedStructures();
            GenerateAudioSources(masterVolume > float.Epsilon);
        }

        private void GenerateAudioSources(bool autostart) {
            foreach (var channel in channels) {
                var go = new GameObject(channel.Name);
                go.transform.SetParent(this.transform, false);
                var ass = go.AddComponent<AudioSource>();
                ass.loop = true;
                ass.clip = channel.Clip;
                channel.AudioSource = ass;

                // to do : maybe have an enum with presets here.
                ApplyPreset(ass);

                if (autostart) channel.StartPlayingAudio();
            }
        }

        private void ApplyPreset(AudioSource ass) {
            if (preset != null) preset.Apply(ass);
        }

        private void CreateFeedStructures() {
            if (feeds.Distinct().Count() != feeds.Length) Debug.LogWarning($"Identical feed names in feeds {name} - complex emmitter will not behave properly");
            feedIDLookup = new Dictionary<string, int>();
            for (var i = 0; i < feeds.Length; i++) feedIDLookup[feeds[i]] = i;
            feedValues = new float[feeds.Length];
        }

        private void LateUpdate() {
            if (dirty) {
                RecalculateFeeds();
                dirty = false;
            }
        }

        private void RecalculateFeeds() {
            foreach (var c in channels) c.Reset();
            foreach (var influence in influences) influence.Apply(this);
            foreach (var c in channels) c.Apply(this);
        }

        private Channel GetChannel(int index) => channels[index];

        private float _SafeGetFeedValue(int index) {
            if (index < 0 || index >= feeds.Length) return 0f;
            return feedValues[index];
        }

        private void _SafeSetFeedValue(int index, float value) {
            if (index < 0 || index >= feeds.Length) return;
            if (Mathf.Approximately(feedValues[index], value)) return;
            feedValues[index] = value;
            dirty = true;
        }

        private int LookupFeedIndex(string id) {
            if (feedIDLookup.TryGetValue(id, out var index)) return index;
            Debug.LogWarning($"Unknown feed referenced in {name} : `{id}`");
            return -1;
        }
    }
}
