using UnityEngine;

namespace Flappy.Helpers
{
    public class BestScore : MonoBehaviour
    {
        // Saves BestScore in PlayerPrefs, then we fetch it in GameManager.cs
        public bool NewBestScore = false; // For the NewBest Sprite

        public void SaveBestScore(int Score)
        {
            if (Score > PlayerPrefs.GetInt("BestScore"))
            {
                NewBestScore = true;
                PlayerPrefs.SetInt("BestScore", Score); // saving Bestscore
            }
            else 
            {
                NewBestScore = false;
            }
        }
    }
}
