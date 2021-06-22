using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using DG.Tweening;
using Flappy.Audio;

namespace Flappy.UI
{ 
    public class GameUIManager : MonoBehaviour
    {
        // Singleton behaviour
        public static GameUIManager Instance;

        [Header("Score")]
        public Text score;

        [Header("Overlay")]
        public Image fadeOverlay;

        [Header("Background")]
        public Transform ground;
        public float groundMovementDuration = 3f;
        public Ease ease;

        private void Start()
        {
            if (GameUIManager.Instance == null)
                GameUIManager.Instance = this;
            else
                Destroy(this.gameObject);

            // Fade the overlay
            FadeOverlay();

            // Start playing tweens
            PlayGround();
        }

        public void FadeOverlay()
        {
            fadeOverlay.DOFade(0, 1f)
                       .SetDelay(0.3f)
                       .OnComplete(()=> fadeOverlay.gameObject.SetActive(false));
        }

        public void PauseGame()
        {
            //pausedCanvas.SetActive(true);
            Time.timeScale = 0;
        }

        public void ResumeGame()
        {
            //pausedCanvas.SetActive(false);
            Time.timeScale = 1;
        }

        public void PlayGround()
        {
            ground.DOLocalMoveX(-540f, groundMovementDuration)
                  .SetEase(ease)
                  .SetLoops(-1);
        }
    }
}

