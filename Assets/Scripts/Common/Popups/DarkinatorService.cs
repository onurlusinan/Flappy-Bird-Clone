using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Popups
{
    // One instance for the whole app; callers stack requests and the topmost one is what shows.
    public sealed class DarkinatorService : IDisposable
    {
        private readonly Darkinator _instance;
        private readonly List<DarkinatorRequest> _requests = new();

        public DarkinatorService(Darkinator prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("DarkinatorService created without a prefab.");
                return;
            }

            _instance = Object.Instantiate(prefab);
            _instance.name = prefab.name;
            Object.DontDestroyOnLoad(_instance.gameObject);
            _instance.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            _requests.Clear();
            if (_instance != null)
            {
                Object.Destroy(_instance.gameObject);
            }
        }

        public UniTask Show(DarkinatorRequest request)
        {
            if (_instance == null || request?.Config == null)
            {
                Debug.LogError("Darkinator shown without an instance or a config.");
                return UniTask.CompletedTask;
            }

            var wasVisible = _requests.Count > 0;
            _requests.Add(request);

            if (!wasVisible)
            {
                return _instance.Show(request);
            }

            _instance.ShowInstant(request);
            return UniTask.CompletedTask;
        }

        public UniTask Release(DarkinatorRequest request)
        {
            if (_instance == null || request == null)
            {
                return UniTask.CompletedTask;
            }

            var wasTop = _requests.Count > 0 && _requests[^1] == request;
            if (!_requests.Remove(request) || !wasTop)
            {
                return UniTask.CompletedTask;
            }

            if (_requests.Count == 0)
            {
                return _instance.Hide(request.Config);
            }

            _instance.ShowInstant(_requests[^1]);
            return UniTask.CompletedTask;
        }

        // For callers that mutated a live request's placement, e.g. a popup moving to another slot.
        public void Refresh(DarkinatorRequest request)
        {
            if (_instance == null || _requests.Count == 0 || _requests[^1] != request)
            {
                return;
            }

            _instance.Apply(request);
        }
    }
}
