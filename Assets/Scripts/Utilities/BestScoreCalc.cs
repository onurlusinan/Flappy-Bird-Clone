using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class BestScoreCalc : MonoBehaviour
    {
        // Saves BestScore in PlayerPrefs, then we fetch it in GameManager.cs
        private int Score;

        public void SaveBestScore(int Score)
        {
            if (Score > PlayerPrefs.GetInt("BestScore"))
            {
                PlayerPrefs.SetInt("BestScore", Score); // saving Bestscore
            }
        }
    }
}
