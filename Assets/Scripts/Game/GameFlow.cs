using App;
using Audio;
using Common.Configs;
using Common.Core.Saving;
using Cysharp.Threading.Tasks;
using Game.Bird;
using Game.Pipes;
using Game.UI;
using Scenes;
using UnityEngine;

namespace Game
{
    // Owns the round's state machine: Ready -> Playing <-> Paused -> Dead. Everything else reacts to it.
    public sealed class GameFlow : MonoBehaviour
    {
        [SerializeField] private BirdController bird;
        [SerializeField] private PipeSpawner pipeSpawner;
        [SerializeField] private GameHud hud;
        [SerializeField] private MedalTable medalTable;
        [SerializeField] private DarkinatorConfig pauseDarkinatorConfig;

        // Background and ground scroll animations, frozen on death.
        [SerializeField] private Animator[] scrollingAnimators;

        private ApplicationContext _app;
        private PausePopupData _pausePopup;
        private int _score;

        public GameState State { get; private set; }

        private void Update()
        {
            if (_app == null)
            {
                return;
            }

            switch (State)
            {
                case GameState.Ready:
                    if (GameInput.TapPressedThisFrame())
                    {
                        StartRound();
                    }

                    break;
                case GameState.Playing:
                    if (GameInput.PausePressedThisFrame())
                    {
                        Pause();
                    }
                    else if (GameInput.TapPressedThisFrame())
                    {
                        Flap();
                    }

                    break;
                case GameState.Paused:
                    if (GameInput.PausePressedThisFrame())
                    {
                        _app.PopupService.ClosePopup(_pausePopup).Forget();
                    }

                    break;
            }
        }

        private void OnDestroy()
        {
            bird.Scored -= HandleScored;
            bird.Died -= HandleDied;
        }

        public void Initialize(ApplicationContext app)
        {
            _app = app;
            bird.Initialize();
            bird.Scored += HandleScored;
            bird.Died += HandleDied;

            State = GameState.Ready;
            _score = 0;
            hud.SetScore(_score);
            hud.ShowReady();
        }

        // The first tap also flaps, so the round never opens with the bird dropping unprompted.
        private void StartRound()
        {
            State = GameState.Playing;
            hud.ShowPlaying();
            bird.Launch();
            pipeSpawner.Begin();
            Flap();
        }

        private void Flap()
        {
            bird.Flap();
            _app.Audio.Play(SoundId.Wing);
        }

        private void Pause()
        {
            State = GameState.Paused;
            Time.timeScale = 0f;

            _pausePopup = new PausePopupData
            {
                HasDarkinator = true,
                DarkinatorConfig = pauseDarkinatorConfig,
                OnClosed = Resume,
            };
            _app.PopupService.ShowPopup(_pausePopup).Forget();
        }

        // Runs from the popup's OnClosed, whichever way it was closed.
        private void Resume()
        {
            Time.timeScale = 1f;
            _pausePopup = null;

            if (State == GameState.Paused)
            {
                State = GameState.Playing;
            }
        }

        private void HandleScored()
        {
            _score++;
            hud.SetScore(_score);
            _app.Audio.Play(SoundId.Point);
        }

        private void HandleDied(DeathCause cause)
        {
            State = GameState.Dead;

            _app.Audio.Play(SoundId.Hit);
            if (cause == DeathCause.Obstacle)
            {
                _app.Audio.Play(SoundId.Die);
            }

            pipeSpawner.Stop();
            foreach (var scrollingAnimator in scrollingAnimators)
            {
                scrollingAnimator.enabled = false;
            }

            ShowGameOver();
        }

        private void ShowGameOver()
        {
            hud.HideScore();
            _app.Audio.Play(SoundId.Swoosh);

            var isNewBest = _app.BestScore.Submit(_score);
            if (isNewBest)
            {
                _app.SaveService.Save(SaveService.DefaultSlotId);
            }

            _app.PopupService.ShowPopup(new GameOverPopupData
            {
                Score = _score,
                BestScore = _app.BestScore.Best,
                IsNewBest = isNewBest,
                Medal = medalTable.Find(_score),
                OnReplay = Replay,
            }).Forget();
        }

        private void Replay()
        {
            _app.Audio.Play(SoundId.Swoosh);
            _app.SceneManager.SwitchTo(SceneId.Game).Forget();
        }
    }
}
