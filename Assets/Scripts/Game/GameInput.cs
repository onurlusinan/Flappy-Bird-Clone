using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game
{
    // Polls the Input System devices directly; taps that land on interactive UI don't count as flaps.
    public static class GameInput
    {
        public static bool TapPressedThisFrame()
        {
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame && !IsPointerOverUI())
            {
                return true;
            }

            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        }

        public static bool PausePressedThisFrame()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
        }

        private static bool IsPointerOverUI()
        {
            var eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject();
        }
    }
}
