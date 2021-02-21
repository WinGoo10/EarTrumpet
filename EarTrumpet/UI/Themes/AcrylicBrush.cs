using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace EarTrumpet.UI.Themes
{
    class AcrylicBrush
    {
        public static string GetBackground(DependencyObject obj) => (string)obj.GetValue(BackgroundProperty);
        public static void SetBackground(DependencyObject obj, string value) => obj.SetValue(BackgroundProperty, value);
        public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.RegisterAttached("Background", typeof(string), typeof(AcrylicBrush), new PropertyMetadata("", BackgroundChanged));

        public static bool GetIsSuppressed(DependencyObject obj) => (bool)obj.GetValue(IsSuppressedProperty);
        public static void SetIsSuppressed(DependencyObject obj, bool value) => obj.SetValue(IsSuppressedProperty, value);
        public static readonly DependencyProperty IsSuppressedProperty =
        DependencyProperty.RegisterAttached("IsSuppressed", typeof(bool), typeof(AcrylicBrush), new PropertyMetadata(false));
        
        private static void BackgroundChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = (Window)dependencyObject;
            var locationChangedTimer = new DispatcherTimer();

            window.Closed += (_, __) => window = null;
            window.SourceInitialized += (_, __) => ApplyAcrylicToWindow(window, (string)e.NewValue);
            window.LocationChanged += (_, __) =>
            {
                if ((HwndSource)PresentationSource.FromVisual(window) != null)
                {
                    if (!locationChangedTimer.IsEnabled)
                    {
                        SetIsSuppressed(window, true);
                        Dispatcher.CurrentDispatcher.InvokeAsync(() => UpdateWindowAcrylic(window));
                    }
                    locationChangedTimer.Stop();
                    locationChangedTimer.Start();
                }
            };

            locationChangedTimer.Interval = TimeSpan.FromMilliseconds(200);
            locationChangedTimer.Tick += (_, __) =>
            {
                locationChangedTimer.Stop();
                SetIsSuppressed(window, false);
                UpdateWindowAcrylic(window);
            };

            Manager.Current.ThemeChanged += () =>
            {
                if (window != null)
                {
                    ApplyAcrylicToWindow(window, (string)e.NewValue);
                }
            };
        }

        private static void ApplyAcrylicToWindow(Window window, string refValue)
        {
            AccentPolicyLibrary.EnableAcrylic(window,
                Manager.Current.ResolveRef(window, refValue),
                Interop.User32.AccentFlags.DrawAllBorders);
        }

        private static void UpdateWindowAcrylic(Window window)
        {
            var suppressed = GetIsSuppressed(window);
            if (suppressed) 
            {
                AccentPolicyLibrary.DisableAcrylic(window);
            } 
            else 
            {
                ApplyAcrylicToWindow(window, GetBackground(window)); 
            }
        }
    }
}
