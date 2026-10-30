using System;
using System.Collections.Generic;

namespace Common.Core.Saving
{
    [Serializable]
    public sealed class SaveFile
    {
        public int version = 1;
        public string savedAtUtc;
        public List<SaveSection> sections = new();
    }

    // A flat list rather than a Dictionary, which JsonUtility can't serialize.
    [Serializable]
    public sealed class SaveSection
    {
        public string key;
        public string json;
    }
}