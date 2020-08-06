using UnityEngine;

namespace Assets.Scripts.Utilities
{
    [System.Serializable]
    public class Sound
    {
        public string name;

        public AudioClip Clip;

        [Range(0f, 1f)]
        public float Volume;
        [Range(0.1f, 3f)]
        public float Pitch;

        [HideInInspector]
        public AudioSource Source;
    }
}
