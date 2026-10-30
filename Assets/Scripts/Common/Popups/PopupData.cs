using System;
using Common.Configs;
using Cysharp.Threading.Tasks;

namespace Common.Popups
{
    // The handle: created with new by the caller, and it outlives the prefab instance.
    public abstract class PopupData
    {
        private readonly UniTaskCompletionSource _activeSource = new();
        private readonly UniTaskCompletionSource _closedSource = new();

        public bool HasDarkinator { get; set; }
        public DarkinatorConfig DarkinatorConfig { get; set; }
        public PopupHideStrategy HideStrategy { get; set; } = PopupHideStrategy.HidePrevious;
        public bool AllowDuplicates { get; set; }

        // False for popups that must be answered, so a stray tap on the overlay can't dismiss them.
        public bool CloseOnDarkinatorClick { get; set; } = true;
        public Action OnClosed { get; set; }

        public PopupState State { get; private set; } = PopupState.Displaying;

        // Reserved by PopupService before the instance exists, so same-frame shows still order correctly.
        public int SortingOrderBase { get; internal set; }

        internal Popup Instance { get; set; }
        internal DarkinatorRequest DarkinatorRequest { get; set; }
        internal UniTaskCompletionSource HideCompletion { get; set; }

        public UniTask WaitUntilActive()
        {
            return _activeSource.Task;
        }

        public UniTask WaitUntilClosed()
        {
            return _closedSource.Task;
        }

        internal void SetState(PopupState state)
        {
            State = state;

            if (state == PopupState.Active)
            {
                _activeSource.TrySetResult();
            }
            else if (state == PopupState.Destroyed)
            {
                // Completed even if it never showed, so a waiter can't deadlock on a popup that failed.
                _activeSource.TrySetResult();
                _closedSource.TrySetResult();
            }
        }
    }
}
