using UnityEngine;

namespace Overlay.Common
{
    /// <summary>
    /// ScriptableObject for storing overlay configuration.
    /// Create via Assets > Create > Overlay > Settings.
    /// </summary>
    [CreateAssetMenu(fileName = "OverlaySettings", menuName = "Overlay/Settings", order = 1)]
    public class OverlaySettings : ScriptableObject
    {
        [Header("Window Behavior")]
        [Tooltip("Start the application in overlay mode with transparency")]
        public bool StartAsOverlay = true;

        [Tooltip("Allow mouse clicks to pass through the window")]
        public bool StartClickThrough = false;

        [Tooltip("Keep the window always on top of other windows")]
        public bool AlwaysOnTop = true;

        [Tooltip("Hide the window from the taskbar")]
        public bool HideFromTaskbar = false;

        [Header("Transparency")]
        [Tooltip("Window opacity (0 = fully transparent, 1 = fully opaque)")]
        [Range(0f, 1f)]
        public float WindowAlpha = 1f;

        [Tooltip("Color to be rendered as transparent (usually black for camera clear color)")]
        public Color TransparentColor = Color.black;

        [Header("Position")]
        [Tooltip("Remember window position between sessions")]
        public bool RememberPosition = true;

        [Tooltip("Default window position when first launched")]
        public Vector2Int DefaultPosition = new Vector2Int(100, 100);

        [Tooltip("Default window size")]
        public Vector2Int DefaultSize = new Vector2Int(400, 300);

        [Header("Drag Behavior")]
        [Tooltip("Allow dragging the window with Alt+Click")]
        public bool EnableDrag = true;

        [Tooltip("Key to hold while clicking to drag the window")]
        public KeyCode DragModifierKey = KeyCode.LeftAlt;

        [Header("Hotkeys")]
        [Tooltip("Key to toggle click-through mode")]
        public KeyCode ToggleClickThroughKey = KeyCode.F1;

        [Tooltip("Key to toggle always-on-top mode")]
        public KeyCode ToggleAlwaysOnTopKey = KeyCode.F2;

        [Tooltip("Key to toggle overlay/normal window mode")]
        public KeyCode ToggleTransparencyKey = KeyCode.F3;
    }
}
