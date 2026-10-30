using System;
using UnityEngine;

namespace Game.UI
{
    [Serializable]
    public sealed class MedalTier
    {
        [SerializeField] private int minScore;
        [SerializeField] private Sprite sprite;

        public int MinScore => minScore;
        public Sprite Sprite => sprite;
    }
}
