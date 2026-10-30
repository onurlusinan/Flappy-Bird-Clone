using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Core.Pooling
{
    // Plain C# pool keyed by an enum; idle instances live under a deactivated root.
    public sealed class PoolService<TKey> : IDisposable where TKey : struct, Enum
    {
        private readonly Dictionary<TKey, Stack<PoolableObject<TKey>>> _pools = new();
        private readonly Dictionary<TKey, GameObject> _prefabs = new();
        private readonly Transform _root;

        // persistent: true survives scene loads; scene-owned pools pass false and Dispose().
        public PoolService(bool persistent = false, string name = null)
        {
            var rootObject = new GameObject(name ?? $"{typeof(TKey).Name}PoolService");
            rootObject.SetActive(false);
            if (persistent)
            {
                Object.DontDestroyOnLoad(rootObject);
            }

            _root = rootObject.transform;
        }

        public void Dispose()
        {
            foreach (var key in new List<TKey>(_pools.Keys)) Deregister(key);

            if (_root != null)
            {
                Object.Destroy(_root.gameObject);
            }
        }

        public void Register(TKey key, GameObject prefab, int preWarmCount = 0)
        {
            if (_prefabs.ContainsKey(key))
            {
                Debug.LogError($"Pool key '{key}' is already registered.");
                return;
            }

            if (prefab == null)
            {
                Debug.LogError($"Cannot register pool key '{key}' with a null prefab.");
                return;
            }

            _prefabs[key] = prefab;
            _pools[key] = new Stack<PoolableObject<TKey>>();

            for (var i = 0; i < preWarmCount; i++) Release(Get(key, _root));
        }

        public PoolableObject<TKey> Get(TKey key, Transform parent = null)
        {
            if (!_pools.TryGetValue(key, out var stack))
            {
                Debug.LogError($"No pool registered for key '{key}'.");
                return null;
            }

            var instance = stack.Count > 0 ? stack.Pop() : Create(key);
            if (instance == null)
            {
                return null;
            }

            instance.transform.SetParent(parent, false);
            instance.gameObject.SetActive(true);
            instance.OnCheckedOut();
            return instance;
        }

        public void Release(PoolableObject<TKey> instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!instance.IsActivelyUsed)
            {
                Debug.LogWarning($"Double release of pooled object '{instance.name}' ignored.");
                return;
            }

            instance.OnReturned();

            if (!_pools.TryGetValue(instance.Key, out var stack))
            {
                Object.Destroy(instance.gameObject);
                return;
            }

            instance.transform.SetParent(_root, false);
            instance.gameObject.SetActive(false);
            stack.Push(instance);
        }

        public void Deregister(TKey key)
        {
            if (!_pools.TryGetValue(key, out var stack))
            {
                return;
            }

            while (stack.Count > 0) Object.Destroy(stack.Pop().gameObject);

            _pools.Remove(key);
            _prefabs.Remove(key);
        }

        private PoolableObject<TKey> Create(TKey key)
        {
            var instance = Object.Instantiate(_prefabs[key], _root);
            var poolable = instance.GetComponent<PoolableObject<TKey>>();
            if (poolable != null)
            {
                poolable.AttachToPool(this, key);
                return poolable;
            }

            Debug.LogError($"Prefab for pool key '{key}' has no PoolableObject<{typeof(TKey).Name}> component.");
            Object.Destroy(instance);
            return null;
        }
    }
}