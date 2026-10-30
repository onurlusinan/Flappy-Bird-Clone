using System;
using System.Collections.Generic;
using Common.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Common.Popups
{
    // Owns the popup stack, the sorting band reservation, and darkinator hand-off.
    public sealed class PopupService : IDisposable
    {
        private readonly PopupServiceConfig _config;
        private readonly DarkinatorService _darkinatorService;
        private readonly Dictionary<Type, Popup> _prefabs = new();
        private readonly List<PopupData> _stack = new();

        public PopupService(PopupCatalog catalog, PopupServiceConfig config, DarkinatorService darkinatorService)
        {
            _config = config;
            _darkinatorService = darkinatorService;
            RegisterPrefabs(catalog);
            UnitySceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        public PopupData CurrentPopup => _stack.Count > 0 ? _stack[^1] : null;
        public bool HasOpenPopup => _stack.Count > 0;

        public void Dispose()
        {
            UnitySceneManager.sceneUnloaded -= HandleSceneUnloaded;
            DestroyAll();
        }

        public async UniTask<bool> ShowPopup(PopupData data)
        {
            if (data == null)
            {
                Debug.LogError("ShowPopup called with null data.");
                return false;
            }

            var type = data.GetType();
            if (!_prefabs.TryGetValue(type, out var prefab))
            {
                Debug.LogError($"No popup prefab registered for '{type.Name}'.");
                return false;
            }

            if (!data.AllowDuplicates && IsOpen(type))
            {
                Debug.LogWarning($"'{type.Name}' is already open; request dropped.");
                return false;
            }

            var below = CurrentPopup;
            if (!TryReserveSortingBand(data, below))
            {
                return false;
            }

            var instance = Object.Instantiate(prefab);
            instance.name = prefab.name;
            data.Instance = instance;
            _stack.Add(data);
            data.SetState(PopupState.Displaying);
            instance.Initialize(data, this);

            // Pushed before the popup underneath releases its own, so the overlay never blinks out.
            await ShowDarkinator(data);

            if (below != null && data.HideStrategy == PopupHideStrategy.HidePrevious)
            {
                await HideInternal(below, false);
            }

            if (data.State != PopupState.Displaying)
            {
                return false;
            }

            await instance.Display();

            if (data.State == PopupState.Displaying)
            {
                data.SetState(PopupState.Active);
            }

            return data.State == PopupState.Active;
        }

        public async UniTask ClosePopup(PopupData data)
        {
            if (data == null || data.State == PopupState.Destroyed)
            {
                return;
            }

            var index = _stack.IndexOf(data);
            if (index < 0)
            {
                return;
            }

            var wasTop = index == _stack.Count - 1;

            await HideInternal(data, true);

            _stack.RemoveAt(index);
            DestroyInstance(data);
            data.SetState(PopupState.Destroyed);
            data.OnClosed?.Invoke();

            // Survivors above the gap move down a slot, which only works because the band is idempotent.
            Reslot(index);

            if (wasTop && _stack.Count > 0)
            {
                await Redisplay(_stack[^1]);
            }
        }

        public UniTask CloseTopPopup()
        {
            return CurrentPopup != null ? ClosePopup(CurrentPopup) : UniTask.CompletedTask;
        }

        public bool IsOpen(Type dataType)
        {
            foreach (var data in _stack)
            {
                if (data.GetType() == dataType)
                {
                    return true;
                }
            }

            return false;
        }

        private void RegisterPrefabs(PopupCatalog catalog)
        {
            if (catalog == null)
            {
                Debug.LogError("PopupService created without a catalog.");
                return;
            }

            foreach (var prefab in catalog.Prefabs)
            {
                if (prefab == null)
                {
                    continue;
                }

                if (!_prefabs.TryAdd(prefab.DataType, prefab))
                {
                    Debug.LogError($"Two popup prefabs claim '{prefab.DataType.Name}'; '{prefab.name}' ignored.");
                }
            }
        }

        // Reserved synchronously so two shows issued in the same frame still get ordered bands.
        private bool TryReserveSortingBand(PopupData data, PopupData below)
        {
            if (_stack.Count >= _config.Capacity)
            {
                Debug.LogError($"Popup stack is full ({_config.Capacity} popups); request dropped.");
                return false;
            }

            data.SortingOrderBase = SortingOrderAbove(below);
            return true;
        }

        private int SortingOrderAbove(PopupData below)
        {
            return below != null ? below.SortingOrderBase + _config.SortingStep : _config.BaseSortingOrder;
        }

        private void Reslot(int fromIndex)
        {
            for (var i = fromIndex; i < _stack.Count; i++)
            {
                var data = _stack[i];
                data.SortingOrderBase = SortingOrderAbove(i > 0 ? _stack[i - 1] : null);

                if (data.Instance != null)
                {
                    data.Instance.ApplySortingOrder();
                }

                if (data.DarkinatorRequest == null)
                {
                    continue;
                }

                data.DarkinatorRequest.SortingOrder = DarkinatorSortingOrder(data);
                _darkinatorService.Refresh(data.DarkinatorRequest);
            }
        }

        private async UniTask HideInternal(PopupData data, bool isForClose)
        {
            if (data.State is PopupState.Hidden or PopupState.Destroyed)
            {
                return;
            }

            if (data.State == PopupState.Hiding)
            {
                await data.HideCompletion.Task;
                return;
            }

            data.SetState(PopupState.Hiding);
            data.HideCompletion = new UniTaskCompletionSource();

            await ReleaseDarkinator(data);

            if (data.Instance != null)
            {
                await data.Instance.Hide(isForClose);
            }

            if (data.State == PopupState.Hiding)
            {
                data.SetState(PopupState.Hidden);
            }

            data.HideCompletion.TrySetResult();
        }

        private async UniTask Redisplay(PopupData data)
        {
            if (data.State != PopupState.Hidden || data.Instance == null)
            {
                return;
            }

            data.SetState(PopupState.Displaying);
            await ShowDarkinator(data);
            await data.Instance.Display();

            if (data.State == PopupState.Displaying)
            {
                data.SetState(PopupState.Active);
            }
        }

        private UniTask ShowDarkinator(PopupData data)
        {
            if (!data.HasDarkinator)
            {
                return UniTask.CompletedTask;
            }

            if (data.DarkinatorConfig == null)
            {
                Debug.LogError($"'{data.GetType().Name}' wants a darkinator but has no config.");
                return UniTask.CompletedTask;
            }

            data.DarkinatorRequest = new DarkinatorRequest
            {
                Config = data.DarkinatorConfig,
                OnClicked = data.CloseOnDarkinatorClick ? () => ClosePopup(data).Forget() : null,
                SortingOrder = DarkinatorSortingOrder(data),
            };

            return _darkinatorService.Show(data.DarkinatorRequest);
        }

        private UniTask ReleaseDarkinator(PopupData data)
        {
            var request = data.DarkinatorRequest;
            if (request == null)
            {
                return UniTask.CompletedTask;
            }

            data.DarkinatorRequest = null;
            return _darkinatorService.Release(request);
        }

        // Inside its own popup's band: behind the popup, in front of everything below it.
        private static int DarkinatorSortingOrder(PopupData data)
        {
            return data.SortingOrderBase - 1;
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            DestroyAll();
        }

        private void DestroyAll()
        {
            for (var i = _stack.Count - 1; i >= 0; i--)
            {
                var data = _stack[i];
                ReleaseDarkinator(data).Forget();
                DestroyInstance(data);
                data.SetState(PopupState.Destroyed);
                data.OnClosed?.Invoke();
            }

            _stack.Clear();
        }

        private static void DestroyInstance(PopupData data)
        {
            if (data.Instance != null)
            {
                Object.Destroy(data.Instance.gameObject);
            }

            data.Instance = null;
        }
    }
}
