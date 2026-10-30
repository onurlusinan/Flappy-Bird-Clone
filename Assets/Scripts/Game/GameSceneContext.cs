using Common.Core;
using Common.Core.Pooling;
using Cysharp.Threading.Tasks;

namespace Game
{
    public sealed class GameSceneContext : Context
    {
        public PoolService<GamePoolKey> Pool { get; private set; }

        public override UniTask Initialize()
        {
            Pool = new PoolService<GamePoolKey>();
            return UniTask.CompletedTask;
        }

        public override UniTask Deactivate()
        {
            Pool.Dispose();
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            Pool.Dispose();
        }
    }
}
