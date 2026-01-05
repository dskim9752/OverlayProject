using System;
using UnityEngine;

namespace Overlay.Common
{
    /// <summary>
    /// Manages the overlay window state and provides a unified API
    /// for controlling the overlay across different platforms.
    /// </summary>
    public class OverlayManager : MonoBehaviour
    {
        #region Singleton

        private static OverlayManager _instance;
        public static OverlayManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<OverlayManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("OverlayManager");
                        _instance = go.AddComponent<OverlayManager>();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Serialized Fields

        [Header("Overlay Settings")]
        [SerializeField] private bool _startAsOverlay = true;
        [SerializeField] private bool _startClickThrough = false;
        [SerializeField] private bool _startAlwaysOnTop = true;
        [SerializeField] private bool _hideFromTaskbar = false;

        [Header("Drag Settings")]
        [SerializeField] private bool _enableDrag = true;
        [SerializeField] private KeyCode _dragModifierKey = KeyCode.LeftAlt;

        [Header("Toggle Keys")]
        [SerializeField] private KeyCode _toggleClickThroughKey = KeyCode.F1;
        [SerializeField] private KeyCode _toggleAlwaysOnTopKey = KeyCode.F2;
        [SerializeField] private KeyCode _toggleTransparencyKey = KeyCode.F3;

        #endregion

        #region Private Fields

        private MonoBehaviour _platformController;
        private bool _isDragging;
        private Vector2 _dragStartMousePos;
        private Vector2Int _dragStartWindowPos;

        #endregion

        #region Properties

        public bool IsOverlayMode { get; private set; }
        public bool IsClickThrough { get; private set; }
        public bool IsAlwaysOnTop { get; private set; }
        public bool IsDragging => _isDragging;

        #endregion

        #region Events

        public event Action<bool> OnOverlayModeChanged;
        public event Action<bool> OnClickThroughChanged;
        public event Action<bool> OnAlwaysOnTopChanged;
        public event Action<Vector2Int> OnWindowMoved;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePlatformController();
        }

        private void Start()
        {
            ApplyInitialSettings();
        }

        private void Update()
        {
            HandleInput();
            HandleDrag();
        }

        #endregion

        #region Initialization

        private void InitializePlatformController()
        {
#if UNITY_STANDALONE_WIN
            var windowsController = gameObject.GetComponent<Windows.TransparentWindowController>();
            if (windowsController == null)
            {
                windowsController = gameObject.AddComponent<Windows.TransparentWindowController>();
            }
            _platformController = windowsController;
            Debug.Log("[OverlayManager] Windows platform controller initialized.");
#elif UNITY_STANDALONE_OSX
            // Mac controller will be added here
            Debug.Log("[OverlayManager] Mac platform - controller not yet implemented.");
#else
            Debug.LogWarning("[OverlayManager] Unsupported platform for overlay functionality.");
#endif
        }

        private void ApplyInitialSettings()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            if (controller == null) return;

            if (_startAsOverlay)
            {
                controller.EnableTransparency();
                IsOverlayMode = true;
            }

            if (_startClickThrough)
            {
                controller.SetClickThrough(true);
                IsClickThrough = true;
            }

            if (_startAlwaysOnTop)
            {
                controller.SetAlwaysOnTop(true);
                IsAlwaysOnTop = true;
            }

            if (_hideFromTaskbar)
            {
                controller.HideFromTaskbar();
            }
#endif
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            if (Input.GetKeyDown(_toggleClickThroughKey))
            {
                ToggleClickThrough();
            }

            if (Input.GetKeyDown(_toggleAlwaysOnTopKey))
            {
                ToggleAlwaysOnTop();
            }

            if (Input.GetKeyDown(_toggleTransparencyKey))
            {
                ToggleOverlayMode();
            }
        }

        private void HandleDrag()
        {
            if (!_enableDrag || IsClickThrough) return;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            if (controller == null) return;

            bool modifierHeld = Input.GetKey(_dragModifierKey);

            if (modifierHeld && Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _dragStartMousePos = Input.mousePosition;
                var (x, y, _, _) = controller.GetWindowRect();
                _dragStartWindowPos = new Vector2Int(x, y);
            }

            if (_isDragging && Input.GetMouseButton(0))
            {
                Vector2 delta = (Vector2)Input.mousePosition - _dragStartMousePos;
                int newX = _dragStartWindowPos.x + (int)delta.x;
                int newY = _dragStartWindowPos.y - (int)delta.y; // Y is inverted
                controller.SetPosition(newX, newY);
                OnWindowMoved?.Invoke(new Vector2Int(newX, newY));
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }
#endif
        }

        #endregion

        #region Public API

        public void ToggleOverlayMode()
        {
            SetOverlayMode(!IsOverlayMode);
        }

        public void SetOverlayMode(bool enabled)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            if (controller == null) return;

            if (enabled)
            {
                controller.EnableTransparency();
            }
            else
            {
                controller.DisableTransparency();
            }

            IsOverlayMode = enabled;
            OnOverlayModeChanged?.Invoke(enabled);
#endif
        }

        public void ToggleClickThrough()
        {
            SetClickThrough(!IsClickThrough);
        }

        public void SetClickThrough(bool enabled)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            if (controller == null) return;

            controller.SetClickThrough(enabled);
            IsClickThrough = enabled;
            OnClickThroughChanged?.Invoke(enabled);
#endif
        }

        public void ToggleAlwaysOnTop()
        {
            SetAlwaysOnTop(!IsAlwaysOnTop);
        }

        public void SetAlwaysOnTop(bool enabled)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            if (controller == null) return;

            controller.SetAlwaysOnTop(enabled);
            IsAlwaysOnTop = enabled;
            OnAlwaysOnTopChanged?.Invoke(enabled);
#endif
        }

        public void SetWindowPosition(int x, int y)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            controller?.SetPosition(x, y);
#endif
        }

        public void SetWindowSize(int width, int height)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            controller?.SetSize(width, height);
#endif
        }

        public void SetWindowAlpha(float alpha)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            controller?.SetWindowAlpha(alpha);
#endif
        }

        public void SetFullScreen()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var controller = _platformController as Windows.TransparentWindowController;
            controller?.SetFullScreen();
#endif
        }

        #endregion
    }
}
