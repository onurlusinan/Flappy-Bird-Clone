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
        public GameObject Bird;
        public GameObject PipeManager;
        public GameObject Background;
        public GameObject Ground;

        private GameObject NewBird;
        private GameObject NewPipeManager;

        public GameObject GetReadyCanvas;
        public GameObject GameOverCanvas;
        public GameObject ScoreCountCanvas;
        public GameObject PausedCanvas;

        private TextMeshProUGUI ScoreCount_TMP;
        private TextMeshProUGUI EndScore_TMP;
        private TextMeshProUGUI BestScore_TMP;

        public Medal[] Medals;

        private int Score;
        private string ScoreStr;

        private bool PipesSpawned; 
        public bool GameOngoing; // After first click, until game is Over
        private bool isGameOver; 
        private bool GameEnded; // we do not want to repeat the gameover function forever. Game Ends after GameOver() is called once
        public bool GamePaused = false;

        [System.Serializable]
        public struct Medal
        {
            public string MedalName;
            public Sprite MedalSprite;
        }

        void Start()
        {
            NewBird = Instantiate(Bird); //Instantiate Flappy Bird 
            NewBird.GetComponent<Rigidbody2D>().isKinematic = true;
            NewBird.transform.rotation = Quaternion.identity;

            PipesSpawned = false;
            GameOngoing = false;
            isGameOver = false;
            GameEnded = false;

            GetReadyCanvas.SetActive(true);
            ScoreCountCanvas.SetActive(true);
            GameOverCanvas.SetActive(false);
            PausedCanvas.SetActive(false);

            ScoreCount_TMP = ScoreCountCanvas.GetComponentInChildren<TextMeshProUGUI>();   
        }

        private void GameOver()
        {
            NewPipeManager.GetComponent<PipeManager>().PipeSpeed = 0; // Pipes Stop

            Background.GetComponent<Animator>().enabled = false;  // Animations Stop
            Ground.GetComponent<Animator>().enabled = false;
            NewBird.GetComponent<Animator>().enabled = false;

            ScoreCountCanvas.SetActive(false); // Canvases
            GameOverCanvas.SetActive(true);

            SoundManager.Instance.Play(Sounds.swoosh);

            FindObjectOfType<ScoreCalculator>().SaveBestScore(Score); // Save Best Score if better than prev best

            if (FindObjectOfType<ScoreCalculator>().NewBestScore) // New BestScore Sprite
            {
                GameObject.FindGameObjectWithTag("NewBest").GetComponent<Image>().enabled = true;
            }
                
            GiveMedal();

            EndScore_TMP = GameObject.FindGameObjectWithTag("EndScore").GetComponent<TextMeshProUGUI>();
            EndScore_TMP.text = ScoreStr;

            BestScore_TMP = GameObject.FindGameObjectWithTag("BestScore").GetComponent<TextMeshProUGUI>();
            BestScore_TMP.text = PlayerPrefs.GetInt("BestScore").ToString();

            GameOngoing = false;
            GameEnded = true;
        }

        public void GiveMedal()
        {
            if (Score < 10)
            {
                GameObject.FindGameObjectWithTag("Medal").GetComponent<Image>().enabled = false;
            }
            else if (Score >= 10 && Score < 20)
            {
                Medal m = Array.Find(Medals, Medal => Medal.MedalName == "BronzeMedal");
                GameObject.FindGameObjectWithTag("Medal").GetComponent<Image>().sprite = m.MedalSprite;
            }
            else if (Score >= 20 && Score < 30)
            {
                Medal m = Array.Find(Medals, Medal => Medal.MedalName == "SilverMedal");
                GameObject.FindGameObjectWithTag("Medal").GetComponent<Image>().sprite = m.MedalSprite;
            }
            else if (Score >= 30 && Score < 40)
            {
                Medal m = Array.Find(Medals, Medal => Medal.MedalName == "GoldMedal");
                GameObject.FindGameObjectWithTag("Medal").GetComponent<Image>().sprite = m.MedalSprite;
            }
            else
            {
                Medal m = Array.Find(Medals, Medal => Medal.MedalName == "PlatinumMedal");
                GameObject.FindGameObjectWithTag("Medal").GetComponent<Image>().sprite = m.MedalSprite;
            }
        }

        public void Replay()
        {
            GameObject.FindGameObjectWithTag("NewBest").GetComponent<Image>().enabled = false;
            SoundManager.Instance.Play(Sounds.swoosh);
            SceneManager.LoadScene(1);
        }

        public void PauseGame()
        {
            GamePaused = true;
            PausedCanvas.SetActive(true);
            Time.timeScale = 0;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            PausedCanvas.SetActive(false);
            GamePaused = false;
        }

        void Update()
        {
            isGameOver =  NewBird.GetComponent<BirdController>().GameOver;

            Score = NewBird.GetComponent<BirdController>().Score; // Updating the Score on the screen as game is played
            ScoreStr = Score.ToString();
            ScoreCount_TMP.text = ScoreStr;

            if (Input.GetMouseButtonDown(0) && !PipesSpawned) // First Click
            {
                NewBird.GetComponent<Rigidbody2D>().isKinematic = false;

                NewPipeManager = Instantiate(PipeManager);

                GetReadyCanvas.SetActive(false);
                ScoreCountCanvas.SetActive(true);

                GameOngoing = true;
                PipesSpawned = true;
            }

            if (isGameOver && !GameEnded)
            {
                GameOver();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver && GameOngoing) // Pause Game (Before GameOver) && (after first click)
            {
                if (!GamePaused)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
    }
}

