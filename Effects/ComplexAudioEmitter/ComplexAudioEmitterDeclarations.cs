using System;
using UnityEngine;

namespace K3.Effects {

    public interface IFeedReceiver {
        void SetFeed(int feedIndex, float value);
        void SetFeed(string feed, float value);
        float FeedValue { get; set; }
    }

    public interface IChannelMaster {
        float MasterVolume { get; }
    }

    public partial class ComplexAudioEmitter {

        public enum Presets {
            LocalAudio,
            ExternalVeryStrong,
            ExternalMidRange,
        }

        public enum FeedProcessing {
            Pitch,
            Volume,
        }

        /// <summary> A CHANNEL will generate a single audio source. 
        /// Channels names are purely there for human readability purposes - they are not used in code.</summary>
        [Serializable]
        public class Channel {
            #pragma warning disable 649
            [SerializeField] string name;
            [SerializeField] AudioClip clip;
            [SerializeField] [Range(0f, 1f)] float volume = 1f;
            #pragma warning restore 649
            public AudioClip Clip => clip;
            public string Name => name;

            internal AudioSource AudioSource { get; set; }

            internal float CurrentPitch { get; set; }
            internal float CurrentVolume { get; set; }

            internal void Reset() {
                CurrentPitch = 1f;
                CurrentVolume = volume;
            }
            // Applies the channel values to audio source.
            internal void Apply(IChannelMaster master) {
                AudioSource.pitch = CurrentPitch;
                AudioSource.volume = CurrentVolume * master.MasterVolume;
            }

            internal void StartPlayingAudio() {
                if (clip == null) return;
                AudioSource.Play();
                AudioSource.time = UnityEngine.Random.Range(0f, AudioSource.clip.length);
            }
        }

        /// <summary> A FEED INFLUENCE is a single definition of how a single input feed parameter affects a channel.</summary>
        [Serializable]
        public class FeedInfluence {
            #pragma warning disable 649
            [SerializeField] int feedIndex; // the feed value of this index will be evaluated at runtime ...
            [SerializeField] int targetChannel; // ... and will target a channel (also by index) ...
            [SerializeField] FeedProcessing effect; // ... by taking this property of the channel (Pitch, Volume) ...
            [SerializeField] AnimationCurve curve; // ... and multiplying it by the value on the curve (evaluated at "feed value")
            #pragma warning restore 649

            public void Apply(ComplexAudioEmitter e) {
                var val = e.GetFeed(feedIndex);
                var r = curve.Evaluate(val);
                var channel = e.GetChannel(targetChannel);

                switch (effect) {
                    case FeedProcessing.Pitch: channel.CurrentPitch *= r; break;
                    case FeedProcessing.Volume: channel.CurrentVolume *= r; break;
                }
            }

        }
    }
}
