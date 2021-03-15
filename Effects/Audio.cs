using UnityEngine;

namespace K3.Effects
{
    public static class Audio
    {
        public static void Play(SoundBank bank, GameObject atGameObject) {
            if (bank == null) return;
            if (atGameObject == null) return;
            var clip = bank.GetNextClip();
            if (clip == null) return;
            var audioSource = atGameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = atGameObject.AddComponent<AudioSource>();
            audioSource.loop = bank.Looping;
            bank.Propagation.Apply(audioSource);
            audioSource.playOnAwake = false;
            // audioSource.clip = clip;
            audioSource.volume = bank.Volume;
            audioSource.pitch = bank.Pitch;
            audioSource.PlayOneShot(clip);
        }
    }
}
