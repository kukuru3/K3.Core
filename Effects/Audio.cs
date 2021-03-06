using UnityEngine;

namespace K3.Effects
{
    public static class Audio
    {
        public static void Play(SoundBank bank, GameObject atGameObject, float volumeMul = 1f, bool bypass = false) {
            if (bank == null) return;
            if (atGameObject == null) return;
            if (volumeMul <= float.Epsilon) return;
            var clip = bank.GetNextClip();
            if (clip == null) return;
            var audioSource = atGameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = atGameObject.AddComponent<AudioSource>();

            audioSource.bypassEffects = bypass;
            audioSource.bypassListenerEffects = bypass;
            

            audioSource.loop = bank.Looping;
            bank.Propagation.Apply(audioSource);
            audioSource.playOnAwake = false;
            
            audioSource.bypassEffects = bypass;
            audioSource.bypassListenerEffects = bypass;
            // audioSource.clip = clip;
            audioSource.volume = bank.Volume * volumeMul;
            audioSource.pitch = bank.Pitch;
            audioSource.PlayOneShot(clip);
        }
    }
}
