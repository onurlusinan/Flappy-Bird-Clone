using TMPro;
using UnityEngine;

namespace Game.UI
{
    // The always-on canvases: "Get Ready" before the first tap, and the live score counter.
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private GameObject getReady;
        [SerializeField] private GameObject scoreCounter;
        [SerializeField] private TMP_Text scoreText;

        public void ShowReady()
        {
            getReady.SetActive(true);
            scoreCounter.SetActive(true);
        }

        public void ShowPlaying()
        {
            getReady.SetActive(false);
        }

        public void HideScore()
        {
            scoreCounter.SetActive(false);
        }

        public void SetScore(int score)
        {
            scoreText.SetText("{0}", score);
        }
    }
}
