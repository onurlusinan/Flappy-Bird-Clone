using System;
using Common.Core.Saving;
using UnityEngine;

namespace Progress
{
    // Owned by ApplicationContext; whoever submits a new best should save right away so it survives a kill.
    public sealed class BestScoreRecord : ISaveParticipant
    {
        private const string LegacyPlayerPrefsKey = "BestScore";

        public string SaveKey => "best-score";
        public int Best { get; private set; }

        // Returns true when the score beats the previous best.
        public bool Submit(int score)
        {
            if (score <= Best)
            {
                return false;
            }

            Best = score;
            return true;
        }

        // Picks up the best score older builds kept in PlayerPrefs, for players with no save file yet.
        public void ImportLegacy()
        {
            Best = Mathf.Max(Best, PlayerPrefs.GetInt(LegacyPlayerPrefsKey, 0));
        }

        public string CaptureState()
        {
            return JsonUtility.ToJson(new State { best = Best });
        }

        public void RestoreState(string json)
        {
            Best = JsonUtility.FromJson<State>(json).best;
        }

        [Serializable]
        private sealed class State
        {
            public int best;
        }
    }
}
