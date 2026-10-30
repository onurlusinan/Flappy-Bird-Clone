using App;
using Common.Core;
using Cysharp.Threading.Tasks;
using Game.Pipes;
using UnityEngine;

namespace Game
{
    public sealed class GameSceneContextController : ContextController<GameSceneContext>
    {
        [SerializeField] private PipeSpawner pipeSpawner;
        [SerializeField] private GameFlow gameFlow;

        protected override GameSceneContext CreateContext()
        {
            return new GameSceneContext();
        }

        protected override UniTask OnContextReady(GameSceneContext context)
        {
            pipeSpawner.Initialize(context.Pool);
            gameFlow.Initialize(ApplicationContextController.Current);
            return UniTask.CompletedTask;
        }
    }
}
