using System.Collections.Generic;
using UnityEngine;

namespace Flappy.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        public List<AudioSource> sources;

        private AudioDataCollection collection;
        public Dictionary<string, AudioData> AudioDict = new Dictionary<string, AudioData>() { };

        private bool muted;
        private bool paused;
        private float timer;

        /* The SoundManager I created before for other projects that has many abilities such as pitch control etc. 
         * SoundManager creates individual audio sources for each clip to be played.
         * This is in order to prevent usage of only one AudioSource, which means only one pitch and one volume.
         * Dynamically creates AudioSources and destroys them whenever the user sets it (preferably at the start and end of levels) */

        private void Awake()
        {
            if (SoundManager.Instance == null)
                SoundManager.Instance = this;
            else
                Destroy(this.gameObject);

            #region Events
            #endregion

            collection = Resources.Load<AudioDataCollection>("Audio/AudioDataCollection");

            foreach (AudioData audio in collection.GetCollection())
            {
                AudioDict.Add(audio.AudioName, audio);
            }

            CheckMute();

            timer = 0;
            paused = false;

            Play(Sounds.swoosh, true); // for testing
        }

        /// <summary>
        /// Mutes all sources
        /// </summary>
        public void SetMute(bool mute)
        {
            foreach (AudioSource source in sources)
            {
                source.mute = mute;
            }
            muted = mute;
        }

        /// <summary>
        /// Pauses all sources
        /// </summary>
        public void PauseSources()
        {
            foreach (AudioSource source in sources)
            {
                source.Pause();
            }
            paused = true;
        }

        /// <summary>
        /// Resumes all sources
        /// </summary>
        public void ResumeSources()
        {
            if (paused)
            {
                foreach (AudioSource source in sources)
                {
                    source.UnPause();
                }
                paused = false;
            }
        }

        /// <summary>
        /// The standard Play function, plays a clip
        /// </summary>
        /// <param name="audioName">Gets the clip name as parameter, check Sounds.cs</param>
        public void Play(string audioName)
        {
            AudioSource source = PrepareSource(audioName);
            if (!paused)
            {
                source.loop = false;
                if (muted)
                    source.mute = true;
                source.Play();
            }
        }

        /// <summary>
        /// Plays sound every x second
        /// </summary>
        /// <param name="audioName">Name of the AudioData obj</param>
        /// <param name="time"> x </param>
        public void Play(string audioName, float time)
        {
            timer += Time.deltaTime;
            if (timer >= time)
            {
                Play(audioName);
                timer = 0;
            }
        }

        /// <summary>
        /// Takes another boolean parameter for looping
        /// </summary>
        /// <param name="audioName">Name of the AudioData obj</param>
        /// <param name="loop">Boolean for the loop</param>
        public void Play(string audioName, bool loop)
        {
            AudioSource source = PrepareSource(audioName);

            if (loop)
                source.loop = true;
            if (muted)
                source.mute = true;

            source.Play();
        }

        /// <summary>
        /// Checks looping and duplicates, if duplicate clip is playing doesn't play
        /// </summary>
        /// <param name="audioName">Name of the AudioData obj</param>
        /// <param name="loop">Boolean for the loop</param>
        public void Play(string audioName, bool loop, bool unique)
        {
            if (unique && !CheckDuplicateClip(audioName))
            {
                AudioSource source = PrepareSource(audioName);

                if (loop)
                    source.loop = true;
                if (muted)
                    source.mute = true;

                source.Play();
            }
        }

        /// <summary>
        /// Returns true if there is a duplicate clip playing
        /// </summary>
        /// <param name="sound"></param>
        /// <returns>true if dupicate exists, false otherwise</returns>
        private bool CheckDuplicateClip(string sound)
        {
            foreach (AudioSource source in sources)
            {
                if (source.clip.name == sound && source.isPlaying)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Creates and prepares a source, used in the Play method
        /// </summary>
        private AudioSource PrepareSource(string audioName)
        {
            AudioData audio = AudioDict[audioName];
            AudioSource source = GetSource();
            source.clip = audio.Clip;
            source.volume = audio.volume;

            if (!audio.randomPitch)
                source.pitch = audio.pitch;
            else
                source.pitch = UnityEngine.Random.Range(0.9f, 1.3f);

            return source;
        }

        /// <summary>
        /// Stops the clip
        /// </summary>
        public void Stop(string clipName)
        {
            AudioClip clip = AudioDict[clipName].Clip;
            foreach (AudioSource source in sources)
            {
                if (source.clip == clip)
                {
                    source.Stop();
                }
            }
        }

        /// <summary>
        /// This returns the unused source if there is any, if not creates a new one
        /// </summary>
        /// <returns> Either unused or new AudioSource </returns>
        private AudioSource GetSource()
        {
            foreach (AudioSource source in sources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return CreateSource();
        }

        /// <summary>
        /// Creates AudioSource
        /// </summary>
        /// <returns> New AudioSource </returns>
        private AudioSource CreateSource()
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            sources.Add(newSource);
            return newSource;
        }

        /// <summary>
        /// This destroys the unused sources whenever needed, for ex: scene changes
        /// Aims for optimizing the amount of audio sources
        /// </summary>
        /// This method is currently unused since the game flow is not yet implemented
        private void DestroySources()
        {
            List<AudioSource> unusedSources = new List<AudioSource>();
            foreach (AudioSource source in sources)
            {
                if (!source.isPlaying)
                {
                    unusedSources.Add(source);
                }
            }
            foreach (AudioSource unusedSource in unusedSources)
            {
                Debug.Log("Destroying: " + unusedSource.clip.name);
                sources.Remove(unusedSource);
                Destroy(unusedSource);
            }
        }

        /// <summary>
        /// This method is supposed to remember the sound mute selection and behave acccordingly at game launch
        /// </summary>
        private void CheckMute()
        {
            // This method will be implemented when the UI for the sound is also implemented
        }
    }
}
