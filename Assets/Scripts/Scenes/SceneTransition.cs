using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace Scenes
{
    // Fade overlay for scene switches; lives on the persistent Application object and starts opaque.
    public sealed class SceneTransition : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Ease ease = Ease.Linear;

        public UniTask FadeOut()
        {
            return Fade(1f);
        }

        public UniTask FadeIn()
        {
            return Fade(0f);
        }

        // Blocks input for the whole fade so nothing underneath can be tapped mid-switch.
        private async UniTask Fade(float targetAlpha)
        {
            overlay.blocksRaycasts = true;

            await LMotion.Create(overlay.alpha, targetAlpha, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(overlay, static (alpha, target) => target.alpha = alpha)
                .ToUniTask(destroyCancellationToken);

            overlay.blocksRaycasts = targetAlpha > 0f;
        }
    }
}
