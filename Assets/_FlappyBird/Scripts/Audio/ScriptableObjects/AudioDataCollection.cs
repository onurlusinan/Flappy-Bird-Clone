using UnityEngine;

namespace Flappy.Audio
{ 
    // AudioDataCollection holds the AudioData objects
    [CreateAssetMenu(fileName = "AudioDataCollection", menuName = "ScriptableObjects/AudioDataCollection")]
    public class AudioDataCollection : ScriptableObject
    {
        [SerializeField]
        private AudioData[] audioDatas;

        public AudioData[] GetCollection()
        {
            return this.audioDatas;
        }
    }
}
