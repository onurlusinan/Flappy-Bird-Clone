using Cysharp.Threading.Tasks;

namespace Common.Core
{
    public abstract class Context
    {
        public virtual UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }

        // Awaited before the scene unloads, with no cancellation so teardown always completes.
        public virtual UniTask Deactivate()
        {
            return UniTask.CompletedTask;
        }

        public virtual void Dispose()
        {
        }
    }
}