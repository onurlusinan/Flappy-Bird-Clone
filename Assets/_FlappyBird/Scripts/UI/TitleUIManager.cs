using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using DG.Tweening;
using Flappy.Audio;

namespace Flappy.UI
{ 
    public class TitleUIManager : MonoBehaviour
    {
        // Singleton behaviour
        public static TitleUIManager Instance;
        
        [Header("Main Title")]
        public Transform mainTitle;

        [Header("Overlay")]
        public Image fadeOverlay;

        [Header("Background")]
        public Transform ground;
        public float groundMovementDuration = 3f;
        public Ease ease;

        private void Start()
        {
            if (TitleUIManager.Instance == null)
                TitleUIManager.Instance = this;
            else
                Destroy(this.gameObject);

            // Fade the overlay
            FadeOverlay();

            // Start playing tweens
            PlayTitle();
            PlayGround();
        }

        public void FadeOverlay()
        {
            fadeOverlay.DOFade(0, 1f).SetDelay(0.3f).OnComplete(()=> fadeOverlay.gameObject.SetActive(false));
        }

        public void PlayTitle()
        {
            Sequence titleLoop = DOTween.Sequence();
            titleLoop.Append(mainTitle.DOLocalMoveY(580f, 0.3f))
                     .Append(mainTitle.DOLocalMoveY(590f, 0.3f))
                     .SetLoops(-1);
        }

        public void PlayGround()
        {
            ground.DOLocalMoveX(-540f, groundMovementDuration)
                  .SetEase(ease)
                  .SetLoops(-1);
        }

        public void StartGame()
        {
            SoundManager.Instance.Play(Sounds.swoosh);
            SceneManager.LoadScene(1);
        }
    }
}

