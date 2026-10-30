using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Popups
{
    // Prefab-side half of a popup; the prefab root is its own overlay canvas. PopupService owns its lifetime.
    [DisallowMultipleComponent]
    public abstract class Popup : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Button backButton;

        private CancellationTokenSource _animationCts;
        private bool _isClosing;
        private PopupService _service;

        public PopupData Data { get; private set; }
        public abstract Type DataType { get; }

        private void OnDestroy()
        {
            StopAnimation();
        }

        public void RequestClose()
        {
            if (_isClosing)
            {
                return;
            }

            _isClosing = true;
            _service.ClosePopup(Data).Forget();
        }

        internal void Initialize(PopupData data, PopupService service)
        {
            Data = data;
            _service = service;

            ApplySortingOrder();
            popupRoot.SetActive(false);
            SetBackButtonActive(false);

            if (backButton != null)
            {
                backButton.onClick.AddListener(RequestClose);
            }

            Setup();
            Validate();
        }

        internal void ApplySortingOrder()
        {
            canvas.sortingOrder = Data.SortingOrderBase;
        }

        internal async UniTask Display()
        {
            popupRoot.SetActive(true);
            SetBackButtonActive(true);
            await Run(PlayShow);
        }

        internal async UniTask Hide(bool isForClose)
        {
            SetBackButtonActive(false);
            await Run(PlayHide);

            if (!isForClose)
            {
                popupRoot.SetActive(false);
            }
        }

        protected virtual void Setup()
        {
        }

        // Animations should run on an unscaled scheduler: popups commonly show while Time.timeScale is 0.
        protected virtual UniTask PlayShow(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask PlayHide(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        private void SetBackButtonActive(bool active)
        {
            if (backButton != null)
            {
                backButton.gameObject.SetActive(active);
            }
        }

        private async UniTask Run(Func<CancellationToken, UniTask> animation)
        {
            StopAnimation();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            await animation(_animationCts.Token).SuppressCancellationThrow();
        }

        private void StopAnimation()
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;
        }

        private void Validate()
        {
#if UNITY_EDITOR
            if (!canvas.isRootCanvas)
            {
                Debug.LogError($"'{name}': the popup canvas must be the prefab's root canvas.", this);
            }

            if (popupRoot.transform.parent != transform)
            {
                Debug.LogError($"'{name}': popupRoot must be a direct child of the prefab root.", this);
            }
#endif
        }
    }
}
