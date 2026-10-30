using LitMotion;
using UnityEngine;

namespace Common.Configs
{
    [CreateAssetMenu(fileName = "DarkinatorConfig", menuName = "Common/Popups/Darkinator Config")]
    public sealed class DarkinatorConfig : ScriptableObject
    {
        [SerializeField] [Range(0f, 1f)] private float alpha = 0.6f;
        [SerializeField] private bool enterAnimated = true;
        [SerializeField] private bool exitAnimated = true;
        [SerializeField] private float enterDuration = 0.2f;
        [SerializeField] private float exitDuration = 0.15f;
        [SerializeField] private Ease enterEase = Ease.OutQuad;
        [SerializeField] private Ease exitEase = Ease.InQuad;

        public float Alpha => alpha;
        public bool EnterAnimated => enterAnimated;
        public bool ExitAnimated => exitAnimated;
        public float EnterDuration => enterDuration;
        public float ExitDuration => exitDuration;
        public Ease EnterEase => enterEase;
        public Ease ExitEase => exitEase;
    }
}
