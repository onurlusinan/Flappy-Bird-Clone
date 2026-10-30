using System;
using System.Threading;
using Common.Configs;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Popups
{
    // Full-screen dimmer on its own overlay canvas, so it can slot between any two popups by sorting order.
    [DisallowMultipleComponent]
    public sealed class Darkinator : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image image;
        [SerializeField] private Button button;

        private CancellationTokenSource _animationCts;
        private Action _onClicked;

        private void Awake()
        {
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            StopAnimation();
        }

        public async UniTask Show(DarkinatorRequest request)
        {
            Apply(request);
            gameObject.SetActive(true);

            if (!request.Config.EnterAnimated)
            {
                SetAlpha(request.Config.Alpha);
                return;
            }

            SetAlpha(0f);
            await Animate(request.Config.Alpha, request.Config.EnterDuration, request.Config.EnterEase);
        }

        // Handoff between popups: re-place without re-fading, so the overlay never blinks.
        public void ShowInstant(DarkinatorRequest request)
        {
            StopAnimation();
            Apply(request);
            gameObject.SetActive(true);
            SetAlpha(request.Config.Alpha);
        }

        public async UniTask Hide(DarkinatorConfig config)
        {
            if (config.ExitAnimated)
            {
                var completed = await Animate(0f, config.ExitDuration, config.ExitEase);
                if (!completed)
                {
                    return;
                }
            }

            SetAlpha(0f);
            _onClicked = null;
            gameObject.SetActive(false);
        }

        public void Apply(DarkinatorRequest request)
        {
            _onClicked = request.OnClicked;
            canvas.sortingOrder = request.SortingOrder;
        }

        private void HandleClick()
        {
            _onClicked?.Invoke();
        }

        private void SetAlpha(float alpha)
        {
            var color = image.color;
            image.color = new Color(color.r, color.g, color.b, alpha);
        }

        // Unscaled so it still fades while the game is paused. Returns false when a newer transition cancelled it.
        private async UniTask<bool> Animate(float targetAlpha, float duration, Ease ease)
        {
            StopAnimation();
            _animationCts = new CancellationTokenSource();
            var token = _animationCts.Token;

            var canceled = await LMotion.Create(image.color.a, targetAlpha, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(image, static (alpha, target) =>
                {
                    var color = target.color;
                    target.color = new Color(color.r, color.g, color.b, alpha);
                })
                .ToUniTask(token)
                .SuppressCancellationThrow();

            return !canceled;
        }

        private void StopAnimation()
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;
        }
    }
}
