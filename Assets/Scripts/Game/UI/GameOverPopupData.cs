using System;
using Common.Popups;
using UnityEngine;

namespace Game.UI
{
    public sealed class GameOverPopupData : PopupData
    {
        public int Score { get; set; }
        public int BestScore { get; set; }
        public bool IsNewBest { get; set; }

        // Null when the score didn't reach any medal tier.
        public Sprite Medal { get; set; }
        public Action OnReplay { get; set; }
    }
}
