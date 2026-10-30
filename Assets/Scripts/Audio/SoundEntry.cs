using System;
using UnityEngine;

namespace Audio
{
    [Serializable]
    public sealed class SoundEntry
    {
        [SerializeField] private SoundId id;
        [SerializeField] private AudioClip clip;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] [Range(0.1f, 3f)] private float pitch = 1f;

        public SoundId Id => id;
        public AudioClip Clip => clip;
        public float Volume => volume;
        public float Pitch => pitch;
    }
}
