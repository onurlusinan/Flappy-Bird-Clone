using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Flappy.Gameplay;

namespace Assets.Flappy.Utilities
{ 
    public class StartMainScene : MonoBehaviour // This script is for the Title Scene
    {
        private Button StartButton;
        private GameObject AudioManager;

        private void Start()
        {
            StartButton = gameObject.GetComponent<Button>();
            StartButton.onClick.AddListener(StartGame);
            AudioManager = GameObject.FindGameObjectWithTag("AudioManager");
        }

        public void StartGame() 
        {
            AudioManager.GetComponent<AudioManager>().Play("swoosh");
            SceneManager.LoadScene(1);
        }
    }
}

