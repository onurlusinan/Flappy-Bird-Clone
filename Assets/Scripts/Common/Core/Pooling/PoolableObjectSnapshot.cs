using System;

namespace Common.Core.Pooling
{
    // Hold this instead of a raw reference across an await; IsValid catches recycled instances.
    public readonly struct PoolableObjectSnapshot<TKey> where TKey : struct, Enum
    {
        private readonly PoolableObject<TKey> _item;
        private readonly int _instanceVersion;

        public PoolableObjectSnapshot(PoolableObject<TKey> item)
        {
            _item = item;
            _instanceVersion = item != null ? item.InstanceVersion : 0;
        }

        public bool IsValid =>
            _item != null && _item.IsActivelyUsed && _item.InstanceVersion == _instanceVersion;

        public PoolableObject<TKey> Item => IsValid ? _item : null;
    }
}