using UnityEngine;

namespace Flappy.Audio
{ 
    // AudioData objects hold the info of the sounds
    // All AudioData's individual volumes and pitches can be changed in the editor
    [CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData")]
    public class AudioData : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] clips;

        [SerializeField]
        public float volume = 1.0f;

        [Range (-2, 4)]
        [SerializeField]
        public float pitch = 1.0f;

        [SerializeField]
        public bool randomPitch;

        public string AudioName;
    
        public AudioClip Clip
        {
            get
            {
                AudioClip clip;
                int length = clips.Length;

                if (length > 0)
                {
                    int index = Random.Range(0, length);
                    clip = clips[index];
                }
                else
                {
                    clip = clips[0];
                }

                return clip;
            }
        }
    } 
}


