using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    [CreateAssetMenu(fileName = "MedalTable", menuName = "Game/Medal Table")]
    public sealed class MedalTable : ScriptableObject
    {
        [SerializeField] private List<MedalTier> tiers = new();

        // The highest tier the score reaches, or null below the lowest; tiers may be listed in any order.
        public Sprite Find(int score)
        {
            MedalTier best = null;
            foreach (var tier in tiers)
            {
                if (score >= tier.MinScore && (best == null || tier.MinScore > best.MinScore))
                {
                    best = tier;
                }
            }

            return best?.Sprite;
        }
    }
}
