using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using DG.Tweening;
using Flappy.Audio;
using Flappy.Helpers;

namespace Flappy.UI
{ 
    public class UIManager : MonoBehaviour
    {
        public Transform mainTitle;
        public Image fadeOverlay;

        private Scene currentScene;

        private void Start()
        {
            currentScene = SceneManager.GetActiveScene();

            FadeOverlay();
        
            if(currentScene.buildIndex == 0) 
                PlayTitle();
        }

        public void FadeOverlay()
        {
            fadeOverlay.DOFade(0, 0.2f);
        }

        public void PlayTitle()
        {
            Sequence titleLoop = DOTween.Sequence();
            titleLoop.Append(mainTitle.DOLocalMoveY(580f, 0.2f))
                     .Append(mainTitle.DOLocalMoveY(600f, 0.2f))
                     .SetLoops(-1);
        }

        public void StartGame()
        {
            SoundManager.Instance.Play(Sounds.swoosh);
            SceneManager.LoadScene(1);
        }
    }
}

