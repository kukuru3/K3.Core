using UnityEngine;

namespace K3.Effects {
    class AudioView : Script {
        #pragma warning disable 649
        [SerializeField] SoundBank bank;
        #pragma warning restore 649

        protected override void Init() {
            var audioSource = GetComponent<AudioSource>();
            if ( audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = bank.Looping;
            bank.Propagation.Apply(audioSource);

            audioSource.clip = bank.GetNextClip();
            audioSource.volume = bank.Volume;
            audioSource.pitch = bank.Pitch;
            audioSource.Play();
        }

    }
}
