using Cysharp.Threading.Tasks;

namespace Common.Core
{
    public abstract class ContextController<TContext> : ContextControllerBase where TContext : Context
    {
        private readonly UniTaskCompletionSource _readyCompletionSource = new();
        private TContext Context { get; set; }

        public override UniTask Ready => _readyCompletionSource.Task;

        private void Awake()
        {
            if (!ShouldInitialize())
            {
                return;
            }

            AwakeAsync().Forget();
        }

        protected virtual void OnDestroy()
        {
            Context?.Dispose();
        }

        public override UniTask Deactivate()
        {
            return Context?.Deactivate() ?? UniTask.CompletedTask;
        }

        protected abstract TContext CreateContext();

        // Return false to opt out of this Awake (e.g. a singleton rejecting a duplicate instance).
        protected virtual bool ShouldInitialize()
        {
            return true;
        }

        protected virtual UniTask OnContextReady(TContext context)
        {
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid AwakeAsync()
        {
            Context = CreateContext();
            await Context.Initialize();
            await OnContextReady(Context);
            _readyCompletionSource.TrySetResult();
        }
    }
}