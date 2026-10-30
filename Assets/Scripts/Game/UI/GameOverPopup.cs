using System.Threading;
using Common.Popups;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class GameOverPopup : TypedPopup<GameOverPopupData>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform scoreBoard;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private Image medalImage;
        [SerializeField] private Image newBestImage;
        [SerializeField] private Button replayButton;

        [SerializeField] private float fadeDuration = 0.13f;
        [SerializeField] private float slideDuration = 0.5f;

        // Anchored Y the score board slides in from; well below the screen at the 1080x1920 reference.
        [SerializeField] private float slideFromY = 1502f;
        [SerializeField] private Ease slideEase = Ease.OutQuad;

        private float _scoreBoardRestY;

        protected override void Setup()
        {
            var data = TypedData;
            scoreText.SetText("{0}", data.Score);
            bestScoreText.SetText("{0}", data.BestScore);
            newBestImage.enabled = data.IsNewBest;
            medalImage.enabled = data.Medal != null;
            medalImage.sprite = data.Medal;

            _scoreBoardRestY = scoreBoard.anchoredPosition.y;
            replayButton.onClick.AddListener(HandleReplay);
        }

        protected override async UniTask PlayShow(CancellationToken cancellationToken)
        {
            var fade = LMotion.Create(0f, 1f, fadeDuration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(canvasGroup, static (alpha, target) => target.alpha = alpha)
                .ToUniTask(cancellationToken);

            var slide = LMotion.Create(slideFromY, _scoreBoardRestY, slideDuration)
                .WithEase(slideEase)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(scoreBoard, static (y, target) =>
                {
                    var position = target.anchoredPosition;
                    target.anchoredPosition = new Vector2(position.x, y);
                })
                .ToUniTask(cancellationToken);

            await UniTask.WhenAll(fade, slide);
        }

        private void HandleReplay()
        {
            replayButton.interactable = false;
            TypedData.OnReplay?.Invoke();
        }
    }
}
