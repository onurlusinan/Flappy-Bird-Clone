using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

using Flappy.Pipes;
using Flappy.Helpers;
using Flappy.Audio;

namespace Flappy.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private void Awake()
        {
            if (GameManager.Instance == null)
                GameManager.Instance = this;
            else
                Destroy(this.gameObject);
        }

        private void GameOver()
        {
            // Stop Pipes
        }

        //public void GiveMedal()
        //{
        //    if (score < 10)
        //    {
        //        // No medal
        //    }
        //    else if (score >= 10 && score < 20)
        //    {
        //       // Bronze medal
        //    }
        //    else if (score >= 20 && score < 30)
        //    {
        //       // Silver Medal
        //    }
        //    else if (score >= 30 && score < 40)
        //    {
        //        // Gold Medal
        //    }
        //    else
        //    {
        //       // Platinum Medal
        //    }
        //}

        public void Replay()
        {
            GameObject.FindGameObjectWithTag("NewBest").GetComponent<Image>().enabled = false;
            SoundManager.Instance.Play(Sounds.swoosh);
            SceneManager.LoadScene(1);
        }

        void Update()
        {
            
        }
    }
}

