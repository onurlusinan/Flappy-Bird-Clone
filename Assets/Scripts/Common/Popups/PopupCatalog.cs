using System.Collections.Generic;
using UnityEngine;

namespace Common.Popups
{
    [CreateAssetMenu(fileName = "PopupCatalog", menuName = "Common/Popups/Popup Catalog")]
    public sealed class PopupCatalog : ScriptableObject
    {
        [SerializeField] private List<Popup> prefabs = new();

        public IReadOnlyList<Popup> Prefabs => prefabs;
    }
}
