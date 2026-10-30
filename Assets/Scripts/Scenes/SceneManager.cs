using System.Threading;
using Common.Core;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Scenes
{
    // Owned by ApplicationContext; Unity's own SceneManager is fully-qualified below.
    public sealed class SceneManager
    {
        private readonly SceneTransition _transition;
        private CancellationTokenSource _sceneCts = new();

        public SceneManager(SceneTransition transition)
        {
            _transition = transition;
        }

        // Cancelled the instant the next SwitchTo begins; scope scene-local work to this.
        public CancellationToken SceneToken => _sceneCts.Token;

        public bool IsSwitching { get; private set; }

        // A request made mid-switch (e.g. a double-tapped button) is dropped rather than queued.
        public async UniTask SwitchTo(SceneId sceneId)
        {
            if (IsSwitching)
            {
                return;
            }

            IsSwitching = true;
            try
            {
                await SwitchToInternal(sceneId);
            }
            finally
            {
                IsSwitching = false;
            }
        }

        private async UniTask SwitchToInternal(SceneId sceneId)
        {
            await _transition.FadeOut();

            var currentController =
                FindControllerInScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            if (currentController != null)
            {
                await currentController.Deactivate();
            }

            _sceneCts.Cancel();
            _sceneCts.Dispose();
            _sceneCts = new CancellationTokenSource();

            var loadOperation =
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)sceneId, LoadSceneMode.Single);
            await loadOperation.ToUniTask(cancellationToken: SceneToken);

            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)sceneId);
            var controller = FindControllerInScene(scene);
            if (controller != null)
            {
                await controller.Ready;
            }

            await _transition.FadeIn();
        }

        // Scoped to the scene's own roots so it can't pick up the persistent ApplicationContext.
        private static ContextControllerBase FindControllerInScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var controller = root.GetComponentInChildren<ContextControllerBase>(true);
                if (controller != null)
                {
                    return controller;
                }
            }

            return null;
        }
    }
}