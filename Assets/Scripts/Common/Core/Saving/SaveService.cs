using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Common.Core.Saving
{
    // Owned by ApplicationContext so it survives scene loads; participants restore on Register() if a slot is loaded.
    public sealed class SaveService
    {
        public const string DefaultSlotId = "slot-1";

        private const string SaveDirectoryName = "Saves";
        private const string SaveFileExtension = ".json";
        private const string ActiveSlotFileName = "active-slot.txt";
        private const int CurrentVersion = 1;

        private readonly List<ISaveParticipant> _participants = new();
        private readonly string _saveDirectory = Path.Combine(Application.persistentDataPath, SaveDirectoryName);

        private SaveFile _loadedFile;

        public void Register(ISaveParticipant participant)
        {
            _participants.Add(participant);
            RestoreParticipant(participant);
        }

        public void Unregister(ISaveParticipant participant)
        {
            _participants.Remove(participant);
        }

        public void Save(string slotId)
        {
            Directory.CreateDirectory(_saveDirectory);

            var file = new SaveFile
            {
                version = CurrentVersion,
                savedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            };

            foreach (var participant in _participants)
                file.sections.Add(new SaveSection { key = participant.SaveKey, json = participant.CaptureState() });

            File.WriteAllText(SlotPath(slotId), JsonUtility.ToJson(file, true));
            SetActiveSlot(slotId);
        }

        public bool Load(string slotId)
        {
            var path = SlotPath(slotId);
            if (!File.Exists(path))
            {
                _loadedFile = null;
                return false;
            }

            _loadedFile = JsonUtility.FromJson<SaveFile>(File.ReadAllText(path));
            foreach (var participant in _participants) RestoreParticipant(participant);
            SetActiveSlot(slotId);
            return true;
        }

        public bool HasSave(string slotId)
        {
            return File.Exists(SlotPath(slotId));
        }

        // Most recently loaded/saved slot, read from a marker file so edit-time tooling can target it.
        public string GetActiveSlotId()
        {
            var path = ActiveSlotPath();
            return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
        }

        public void DeleteSlot(string slotId)
        {
            var path = SlotPath(slotId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public IEnumerable<string> ListSlots()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                yield break;
            }

            foreach (var path in Directory.GetFiles(_saveDirectory, "*" + SaveFileExtension))
                yield return Path.GetFileNameWithoutExtension(path);
        }

        private void RestoreParticipant(ISaveParticipant participant)
        {
            if (_loadedFile == null)
            {
                return;
            }

            foreach (var section in _loadedFile.sections)
            {
                if (section.key != participant.SaveKey)
                {
                    continue;
                }

                participant.RestoreState(section.json);
                return;
            }
        }

        private string SlotPath(string slotId)
        {
            return Path.Combine(_saveDirectory, slotId + SaveFileExtension);
        }

        private void SetActiveSlot(string slotId)
        {
            Directory.CreateDirectory(_saveDirectory);
            File.WriteAllText(ActiveSlotPath(), slotId);
        }

        private string ActiveSlotPath()
        {
            return Path.Combine(_saveDirectory, ActiveSlotFileName);
        }
    }
}