using System.Collections.Generic;
using Common.Core.Pooling;
using UnityEngine;

namespace Game.Pipes
{
    // Spawns pooled pipes at spawnPoint and scrolls them all left from one Update.
    public sealed class PipeSpawner : MonoBehaviour
    {
        [SerializeField] private Pipe pipePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float speed = 2.5f;

        // 1.3s pairs with the 2.5 speed for the classic pipe spacing; change them together.
        [SerializeField] private float spawnInterval = 1.3f;
        [SerializeField] private float heightRange = 2.5f;

        // World X past the camera's left edge at which a pipe is returned to the pool.
        [SerializeField] private float despawnX = -9.5f;
        [SerializeField] private int preWarmCount = 4;

        private readonly List<Pipe> _active = new();
        private PoolService<GamePoolKey> _pool;
        private float _timer;
        private bool _isRunning;

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            ScrollPipes(speed * Time.deltaTime);

            _timer += Time.deltaTime;
            if (_timer >= spawnInterval)
            {
                _timer -= spawnInterval;
                Spawn();
            }
        }

        public void Initialize(PoolService<GamePoolKey> pool)
        {
            _pool = pool;
            _pool.Register(GamePoolKey.Pipe, pipePrefab.gameObject, preWarmCount);
        }

        public void Begin()
        {
            _timer = 0f;
            _isRunning = true;
        }

        // Pipes freeze where they are; the pool takes them back when the scene unloads.
        public void Stop()
        {
            _isRunning = false;
        }

        private void ScrollPipes(float distance)
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var pipe = _active[i];
                pipe.transform.position += Vector3.left * distance;

                if (pipe.transform.position.x >= despawnX)
                {
                    continue;
                }

                _active.RemoveAt(i);
                pipe.Release();
            }
        }

        private void Spawn()
        {
            if (_pool.Get(GamePoolKey.Pipe, transform) is not Pipe pipe)
            {
                return;
            }

            pipe.transform.position = spawnPoint.position + Vector3.up * Random.Range(-heightRange, heightRange);
            _active.Add(pipe);
        }
    }
}
