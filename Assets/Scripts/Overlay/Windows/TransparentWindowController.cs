using System;
using UnityEngine;

namespace Overlay.Windows
{
    /// <summary>
    /// Controls the transparent overlay window on Windows.
    /// Handles window transparency, click-through, always-on-top, and positioning.
    /// </summary>
    public class TransparentWindowController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Window Settings")]
        [SerializeField] private bool _transparentOnStart = true;
        [SerializeField] private bool _clickThroughOnStart = false;
        [SerializeField] private bool _alwaysOnTop = true;
        [SerializeField] private bool _hideFromTaskbar = false;

        [Header("Transparency")]
        [Range(0f, 1f)]
        [SerializeField] private float _windowAlpha = 1f;
        [SerializeField] private Color _transparentColor = Color.black;

        [Header("Position")]
        [SerializeField] private bool _rememberPosition = true;
        [SerializeField] private Vector2Int _defaultPosition = new Vector2Int(100, 100);

        #endregion

        #region Private Fields

        private IntPtr _windowHandle;
        private bool _isTransparent;
        private bool _isClickThrough;
        private uint _originalStyle;
        private uint _originalExStyle;
        private const string POSITION_X_KEY = "OverlayPosX";
        private const string POSITION_Y_KEY = "OverlayPosY";

        #endregion

        #region Properties

        public IntPtr WindowHandle => _windowHandle;
        public bool IsTransparent => _isTransparent;
        public bool IsClickThrough => _isClickThrough;
        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetAlwaysOnTop(value);
        }

        #endregion

        #region Events

        public event Action OnWindowInitialized;
        public event Action<bool> OnTransparencyChanged;
        public event Action<bool> OnClickThroughChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            InitializeWindow();
#else
            Debug.Log("[TransparentWindowController] Running in Editor or non-Windows platform. Window modifications disabled.");
#endif
        }

        private void Start()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (_transparentOnStart)
            {
                EnableTransparency();
            }

            if (_clickThroughOnStart)
            {
                SetClickThrough(true);
            }

            if (_alwaysOnTop)
            {
                SetAlwaysOnTop(true);
            }

            if (_hideFromTaskbar)
            {
                HideFromTaskbar();
            }

            if (_rememberPosition)
            {
                RestorePosition();
            }
#endif
        }

        private void OnApplicationQuit()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (_rememberPosition)
            {
                SavePosition();
            }
#endif
        }

        #endregion

        #region Initialization

        private void InitializeWindow()
        {
            _windowHandle = WindowsAPI.GetActiveWindow();

            if (_windowHandle == IntPtr.Zero)
            {
                Debug.LogError("[TransparentWindowController] Failed to get window handle.");
                return;
            }

            // Store original styles for restoration
            _originalStyle = (uint)WindowsAPI.GetWindowLong(_windowHandle, WindowsAPI.GWL_STYLE);
            _originalExStyle = (uint)WindowsAPI.GetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE);

            Debug.Log($"[TransparentWindowController] Window initialized. Handle: {_windowHandle}");
            OnWindowInitialized?.Invoke();
        }

        #endregion

        #region Transparency

        public void EnableTransparency()
        {
            if (_windowHandle == IntPtr.Zero) return;

            // Remove window frame
            uint style = (uint)WindowsAPI.GetWindowLong(_windowHandle, WindowsAPI.GWL_STYLE);
            style &= ~(WindowsAPI.WS_CAPTION | WindowsAPI.WS_THICKFRAME | WindowsAPI.WS_SYSMENU);
            WindowsAPI.SetWindowLong(_windowHandle, WindowsAPI.GWL_STYLE, style);

            // Add layered window style
            uint exStyle = (uint)WindowsAPI.GetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE);
            exStyle |= WindowsAPI.WS_EX_LAYERED;
            WindowsAPI.SetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE, exStyle);

            // Set color key for transparency (black becomes transparent)
            uint colorKey = (uint)(
                ((int)(_transparentColor.b * 255) << 16) |
                ((int)(_transparentColor.g * 255) << 8) |
                (int)(_transparentColor.r * 255)
            );

            WindowsAPI.SetLayeredWindowAttributes(
                _windowHandle,
                colorKey,
                (byte)(_windowAlpha * 255),
                WindowsAPI.LWA_COLORKEY | WindowsAPI.LWA_ALPHA
            );

            // Extend frame into client area for DWM composition
            if (WindowsAPI.IsDwmCompositionEnabled())
            {
                var margins = new WindowsAPI.MARGINS
                {
                    cxLeftWidth = -1,
                    cxRightWidth = -1,
                    cyTopHeight = -1,
                    cyBottomHeight = -1
                };
                WindowsAPI.DwmExtendFrameIntoClientArea(_windowHandle, ref margins);
            }

            // Force window to update
            WindowsAPI.SetWindowPos(
                _windowHandle,
                IntPtr.Zero,
                0, 0, 0, 0,
                WindowsAPI.SWP_NOMOVE | WindowsAPI.SWP_NOSIZE | WindowsAPI.SWP_FRAMECHANGED
            );

            _isTransparent = true;
            OnTransparencyChanged?.Invoke(true);

            Debug.Log("[TransparentWindowController] Transparency enabled.");
        }

        public void DisableTransparency()
        {
            if (_windowHandle == IntPtr.Zero) return;

            // Restore original styles
            WindowsAPI.SetWindowLong(_windowHandle, WindowsAPI.GWL_STYLE, _originalStyle);
            WindowsAPI.SetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE, _originalExStyle);

            // Force window to update
            WindowsAPI.SetWindowPos(
                _windowHandle,
                IntPtr.Zero,
                0, 0, 0, 0,
                WindowsAPI.SWP_NOMOVE | WindowsAPI.SWP_NOSIZE | WindowsAPI.SWP_FRAMECHANGED
            );

            _isTransparent = false;
            OnTransparencyChanged?.Invoke(false);

            Debug.Log("[TransparentWindowController] Transparency disabled.");
        }

        public void SetWindowAlpha(float alpha)
        {
            _windowAlpha = Mathf.Clamp01(alpha);

            if (_windowHandle == IntPtr.Zero || !_isTransparent) return;

            WindowsAPI.SetLayeredWindowAttributes(
                _windowHandle,
                0,
                (byte)(_windowAlpha * 255),
                WindowsAPI.LWA_ALPHA
            );
        }

        #endregion

        #region Click-Through

        public void SetClickThrough(bool enabled)
        {
            if (_windowHandle == IntPtr.Zero) return;

            uint exStyle = (uint)WindowsAPI.GetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE);

            if (enabled)
            {
                exStyle |= WindowsAPI.WS_EX_TRANSPARENT;
            }
            else
            {
                exStyle &= ~WindowsAPI.WS_EX_TRANSPARENT;
            }

            WindowsAPI.SetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE, exStyle);
            _isClickThrough = enabled;
            OnClickThroughChanged?.Invoke(enabled);

            Debug.Log($"[TransparentWindowController] Click-through: {enabled}");
        }

        public void ToggleClickThrough()
        {
            SetClickThrough(!_isClickThrough);
        }

        #endregion

        #region Always On Top

        public void SetAlwaysOnTop(bool enabled)
        {
            if (_windowHandle == IntPtr.Zero) return;

            _alwaysOnTop = enabled;

            IntPtr insertAfter = enabled ? WindowsAPI.HWND_TOPMOST : WindowsAPI.HWND_NOTOPMOST;

            WindowsAPI.SetWindowPos(
                _windowHandle,
                insertAfter,
                0, 0, 0, 0,
                WindowsAPI.SWP_NOMOVE | WindowsAPI.SWP_NOSIZE | WindowsAPI.SWP_NOACTIVATE
            );

            Debug.Log($"[TransparentWindowController] Always on top: {enabled}");
        }

        #endregion

        #region Taskbar

        public void HideFromTaskbar()
        {
            if (_windowHandle == IntPtr.Zero) return;

            uint exStyle = (uint)WindowsAPI.GetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE);
            exStyle |= WindowsAPI.WS_EX_TOOLWINDOW;
            exStyle &= ~WindowsAPI.WS_EX_APPWINDOW;
            WindowsAPI.SetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE, exStyle);

            Debug.Log("[TransparentWindowController] Hidden from taskbar.");
        }

        public void ShowInTaskbar()
        {
            if (_windowHandle == IntPtr.Zero) return;

            uint exStyle = (uint)WindowsAPI.GetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE);
            exStyle &= ~WindowsAPI.WS_EX_TOOLWINDOW;
            exStyle |= WindowsAPI.WS_EX_APPWINDOW;
            WindowsAPI.SetWindowLong(_windowHandle, WindowsAPI.GWL_EXSTYLE, exStyle);

            Debug.Log("[TransparentWindowController] Shown in taskbar.");
        }

        #endregion

        #region Position & Size

        public void SetPosition(int x, int y)
        {
            if (_windowHandle == IntPtr.Zero) return;

            WindowsAPI.SetWindowPos(
                _windowHandle,
                IntPtr.Zero,
                x, y,
                0, 0,
                WindowsAPI.SWP_NOSIZE | WindowsAPI.SWP_NOACTIVATE
            );
        }

        public void SetSize(int width, int height)
        {
            if (_windowHandle == IntPtr.Zero) return;

            WindowsAPI.SetWindowPos(
                _windowHandle,
                IntPtr.Zero,
                0, 0,
                width, height,
                WindowsAPI.SWP_NOMOVE | WindowsAPI.SWP_NOACTIVATE
            );
        }

        public void SetPositionAndSize(int x, int y, int width, int height)
        {
            if (_windowHandle == IntPtr.Zero) return;

            WindowsAPI.MoveWindow(_windowHandle, x, y, width, height, true);
        }

        public (int x, int y, int width, int height) GetWindowRect()
        {
            if (_windowHandle == IntPtr.Zero) return (0, 0, 0, 0);

            WindowsAPI.GetWindowRect(_windowHandle, out var rect);
            return (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public void SetFullScreen()
        {
            var bounds = WindowsAPI.GetVirtualScreenBounds();
            SetPositionAndSize(
                bounds.Left,
                bounds.Top,
                bounds.Right - bounds.Left,
                bounds.Bottom - bounds.Top
            );
        }

        private void SavePosition()
        {
            var (x, y, _, _) = GetWindowRect();
            PlayerPrefs.SetInt(POSITION_X_KEY, x);
            PlayerPrefs.SetInt(POSITION_Y_KEY, y);
            PlayerPrefs.Save();
        }

        private void RestorePosition()
        {
            int x = PlayerPrefs.GetInt(POSITION_X_KEY, _defaultPosition.x);
            int y = PlayerPrefs.GetInt(POSITION_Y_KEY, _defaultPosition.y);
            SetPosition(x, y);
        }

        #endregion

        #region Window Visibility

        public void ShowWindow()
        {
            if (_windowHandle == IntPtr.Zero) return;
            WindowsAPI.ShowWindow(_windowHandle, WindowsAPI.SW_SHOW);
        }

        public void HideWindow()
        {
            if (_windowHandle == IntPtr.Zero) return;
            WindowsAPI.ShowWindow(_windowHandle, WindowsAPI.SW_HIDE);
        }

        public void MinimizeWindow()
        {
            if (_windowHandle == IntPtr.Zero) return;
            WindowsAPI.ShowWindow(_windowHandle, WindowsAPI.SW_MINIMIZE);
        }

        public void RestoreWindow()
        {
            if (_windowHandle == IntPtr.Zero) return;
            WindowsAPI.ShowWindow(_windowHandle, WindowsAPI.SW_RESTORE);
        }

        #endregion
    }
}
