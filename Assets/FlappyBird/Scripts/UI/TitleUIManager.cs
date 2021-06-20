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
        public static TitleUIManager Instance;

        public Transform mainTitle;
        public Image fadeOverlay;

        private void Start()
        {
            if (TitleUIManager.Instance == null)
                TitleUIManager.Instance = this;
            else
                Destroy(this.gameObject);

            FadeOverlay();
            PlayTitle();
        }

        public void FadeOverlay()
        {
            fadeOverlay.DOFade(0, 0.5f);
        }

        public void PlayTitle()
        {
            Sequence titleLoop = DOTween.Sequence();
            titleLoop.Append(mainTitle.DOLocalMoveY(580f, 0.3f))
                     .Append(mainTitle.DOLocalMoveY(590f, 0.3f))
                     .SetEase(Ease.Linear)
                     .SetLoops(-1);
        }

        public void StartGame()
        {
            SoundManager.Instance.Play(Sounds.swoosh);
            SceneManager.LoadScene(1);
        }
    }
}

