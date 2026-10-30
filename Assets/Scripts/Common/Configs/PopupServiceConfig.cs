using UnityEngine;

namespace Common.Configs
{
    [CreateAssetMenu(fileName = "PopupServiceConfig", menuName = "Common/Popups/Popup Service Config")]
    public sealed class PopupServiceConfig : ScriptableObject
    {
        // Above every scene canvas; each stacked popup takes the next band of sortingStep orders.
        [SerializeField] private int baseSortingOrder = 100;
        [SerializeField] private int sortingStep = 10;
        [SerializeField] private int capacity = 8;

        public int BaseSortingOrder => baseSortingOrder;

        // At least 2: the darkinator sits one order below its popup, inside that popup's band.
        public int SortingStep => Mathf.Max(2, sortingStep);
        public int Capacity => capacity;
    }
}
