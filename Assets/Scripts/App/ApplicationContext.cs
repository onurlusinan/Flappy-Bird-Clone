using Audio;
using Common.Configs;
using Common.Core;
using Common.Core.Saving;
using Common.Popups;
using Cysharp.Threading.Tasks;
using Progress;
using Scenes;

namespace App
{
    public sealed class ApplicationContext : Context
    {
        public ApplicationContext(SceneTransition transition, Darkinator darkinatorPrefab, PopupCatalog popupCatalog,
            PopupServiceConfig popupServiceConfig, AudioPlayer audio)
        {
            SceneManager = new SceneManager(transition);
            DarkinatorService = new DarkinatorService(darkinatorPrefab);
            PopupService = new PopupService(popupCatalog, popupServiceConfig, DarkinatorService);
            Audio = audio;
            SaveService.Register(BestScore);
        }

        public SceneManager SceneManager { get; }
        public SaveService SaveService { get; } = new();
        public DarkinatorService DarkinatorService { get; }
        public PopupService PopupService { get; }
        public AudioPlayer Audio { get; }
        public BestScoreRecord BestScore { get; } = new();

        public override UniTask Initialize()
        {
            if (!SaveService.Load(SaveService.DefaultSlotId))
            {
                BestScore.ImportLegacy();
            }

            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            PopupService.Dispose();
            DarkinatorService.Dispose();
        }
    }
}
