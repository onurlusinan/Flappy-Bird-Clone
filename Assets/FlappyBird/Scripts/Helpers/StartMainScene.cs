using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Flappy.Audio;

namespace Assets.Flappy.Utilities
{ 
    public class StartMainScene : MonoBehaviour // This script is for the Title Scene
    {
        private Button StartButton;

        private void Start()
        {
            StartButton = gameObject.GetComponent<Button>();
            StartButton.onClick.AddListener(StartGame);
        }

        public void StartGame() 
        {
            SoundManager.Instance.Play(Sounds.swoosh);
            SceneManager.LoadScene(1);
        }
    }
}

