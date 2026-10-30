using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    // One AudioSource per sound, so different sounds overlap while replaying one restarts it.
    public sealed class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private List<SoundEntry> sounds = new();

        private readonly Dictionary<SoundId, AudioSource> _sources = new();

        private void Awake()
        {
            foreach (var sound in sounds)
            {
                if (_sources.ContainsKey(sound.Id))
                {
                    Debug.LogError($"Sound '{sound.Id}' is listed twice; the later entry is ignored.", this);
                    continue;
                }

                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.clip = sound.Clip;
                source.volume = sound.Volume;
                source.pitch = sound.Pitch;
                _sources.Add(sound.Id, source);
            }
        }

        public void Play(SoundId id)
        {
            if (_sources.TryGetValue(id, out var source))
            {
                source.Play();
                return;
            }

            Debug.LogWarning($"Sound '{id}' has no entry.", this);
        }
    }
}
