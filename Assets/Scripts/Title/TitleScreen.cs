using App;
using Audio;
using Cysharp.Threading.Tasks;
using Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace Title
{
    public sealed class TitleScreen : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        private void Awake()
        {
            playButton.onClick.AddListener(Play);
        }

        private void Play()
        {
            playButton.interactable = false;
            var app = ApplicationContextController.Current;
            app.Audio.Play(SoundId.Swoosh);
            app.SceneManager.SwitchTo(SceneId.Game).Forget();
        }
    }
}
