using UnityEngine;
using System;

using Flappy.Helpers;

namespace Flappy.Core
{
    public class AudioManager : MonoBehaviour
    {
        public Sound[] Sounds;

        private void Awake() // Before start
        {
            foreach (Sound s in Sounds)
            {
                s.Source = gameObject.AddComponent<AudioSource>();
                s.Source.clip = s.Clip;
                s.Source.volume = s.Volume;
                s.Source.pitch = s.Pitch;
            }

            int AudioManagers = FindObjectsOfType<AudioManager>().Length; // if more then one AudioManager is in the scene
            if (AudioManagers != 1)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void Play(string name)
        {
            Sound s = Array.Find(Sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.Source.Play();
        }
    }
}
