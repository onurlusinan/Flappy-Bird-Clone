using Audio;
using Common.Configs;
using Common.Core;
using Common.Core.Saving;
using Common.Popups;
using Cysharp.Threading.Tasks;
using Scenes;
using UnityEngine;

namespace App
{
    public sealed class ApplicationContextController : ContextController<ApplicationContext>
    {
        [SerializeField] private SceneTransition transition;
        [SerializeField] private Darkinator darkinatorPrefab;
        [SerializeField] private PopupCatalog popupCatalog;
        [SerializeField] private PopupServiceConfig popupServiceConfig;
        [SerializeField] private AudioPlayer audioPlayer;
        private static ApplicationContextController Instance { get; set; }

        // Static bridge for scene controllers, which load after this and have no other way to reach it.
        public static ApplicationContext Current { get; private set; }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Current = null;
            }

            base.OnDestroy();
        }

        private void OnApplicationQuit()
        {
            Current?.SaveService.Save(SaveService.DefaultSlotId);
        }

        protected override bool ShouldInitialize()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return false;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }

        protected override ApplicationContext CreateContext()
        {
            Current = new ApplicationContext(transition, darkinatorPrefab, popupCatalog, popupServiceConfig,
                audioPlayer);
            return Current;
        }

        protected override UniTask OnContextReady(ApplicationContext context)
        {
            return context.SceneManager.SwitchTo(SceneId.Title);
        }
    }
}
