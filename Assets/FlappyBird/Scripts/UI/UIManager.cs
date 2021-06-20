using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

namespace Flappy.UI
{ 
    public class UIManager : MonoBehaviour
    {
        public Transform mainTitle;
        public Image fadeOverlay;

        private void Start()
        {
            FadeOverlay();
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
    }
}

