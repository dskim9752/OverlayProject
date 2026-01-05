using System;

namespace Overlay.Common
{
    /// <summary>
    /// Platform-agnostic interface for overlay window control.
    /// Implemented by Windows and Mac specific controllers.
    /// </summary>
    public interface IOverlayController
    {
        // Properties
        bool IsTransparent { get; }
        bool IsClickThrough { get; }
        bool AlwaysOnTop { get; set; }

        // Events
        event Action OnWindowInitialized;
        event Action<bool> OnTransparencyChanged;
        event Action<bool> OnClickThroughChanged;

        // Transparency
        void EnableTransparency();
        void DisableTransparency();
        void SetWindowAlpha(float alpha);

        // Click-Through
        void SetClickThrough(bool enabled);
        void ToggleClickThrough();

        // Always On Top
        void SetAlwaysOnTop(bool enabled);

        // Taskbar
        void HideFromTaskbar();
        void ShowInTaskbar();

        // Position & Size
        void SetPosition(int x, int y);
        void SetSize(int width, int height);
        void SetPositionAndSize(int x, int y, int width, int height);
        (int x, int y, int width, int height) GetWindowRect();
        void SetFullScreen();

        // Visibility
        void ShowWindow();
        void HideWindow();
        void MinimizeWindow();
        void RestoreWindow();
    }
}
