using System;
using UnityEngine;

namespace Common.Core.Pooling
{
    // Base for every prefab root managed by a PoolService<TKey>.
    public abstract class PoolableObject<TKey> : MonoBehaviour where TKey : struct, Enum
    {
        private PoolService<TKey> _pool;
        public TKey Key { get; private set; }

        // Bumped on every checkout; pairs with PoolableObjectSnapshot to catch stale references.
        public int InstanceVersion { get; private set; }
        public bool IsActivelyUsed { get; private set; }

        public void Release()
        {
            _pool?.Release(this);
        }

        protected abstract void OnGetFromPool();
        protected abstract void OnBackToPool();

        internal void AttachToPool(PoolService<TKey> pool, TKey key)
        {
            _pool = pool;
            Key = key;
        }

        internal void OnCheckedOut()
        {
            InstanceVersion++;
            IsActivelyUsed = true;
            OnGetFromPool();
        }

        internal void OnReturned()
        {
            IsActivelyUsed = false;
            OnBackToPool();
        }
    }
}