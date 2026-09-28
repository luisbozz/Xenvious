using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;

            // An open list scrolls a half-visible entry into view as soon as the mouse is on it;
            // at the bottom edge that runs on from entry to entry and the list jumps. Only the
            // keyboard should scroll the list.
            EventManager.RegisterClassHandler(typeof(ComboBoxItem), FrameworkElement.RequestBringIntoViewEvent,
                new RequestBringIntoViewEventHandler((sender, e) =>
                {
                    if (((ComboBoxItem)sender).IsMouseOver && Mouse.LeftButton == MouseButtonState.Released)
                        e.Handled = true;
                }));

            // Scrolling a page over a closed dropdown changed its value and stopped the page.
            // The wheel only picks in a dropdown that has the focus and when the page was not
            // just scrolled; otherwise it scrolls the page on.
            EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.PreviewMouseWheelEvent,
                new MouseWheelEventHandler((sender, e) =>
                {
                    var box = (ComboBox)sender;
                    if (box.IsDropDownOpen)
                        return;
                    if (box.IsKeyboardFocusWithin && Environment.TickCount - _lastPageScroll > 600)
                        return;
                    e.Handled = true;
                    for (DependencyObject p = System.Windows.Media.VisualTreeHelper.GetParent(box); p != null; p = System.Windows.Media.VisualTreeHelper.GetParent(p))
                        if (p is ScrollViewer sv && sv.ScrollableHeight > 0)
                        {
                            sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
                            _lastPageScroll = Environment.TickCount;
                            break;
                        }
                }));
            EventManager.RegisterClassHandler(typeof(ScrollViewer), ScrollViewer.ScrollChangedEvent,
                new ScrollChangedEventHandler((sender, e) =>
                {
                    if (e.VerticalChange != 0 && Mouse.LeftButton == MouseButtonState.Released)
                        _lastPageScroll = Environment.TickCount;
                }));
        }

        private static int _lastPageScroll = int.MinValue / 2;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Brushes loaded from XAML are frozen; replace every theme brush with an unfrozen copy
            // before the window is built, so a theme change can recolour them in place and
            // StaticResource users and brushes taken in code follow along.
            foreach (string key in Themes.TokenKeys)
                if (TryFindResource(key) is System.Windows.Media.SolidColorBrush brush)
                    Resources[key] = new System.Windows.Media.SolidColorBrush(brush.Color);
            base.OnStartup(e);
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogCrash("Dispatcher", e.Exception);
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception ?? new Exception("Unhandled non-exception object.");
            LogCrash("AppDomain", ex);
        }

        private static void OnTaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogCrash("TaskScheduler", e.Exception);
            e.SetObserved();
        }

        private static void LogCrash(string source, Exception ex)
        {
            try
            {
                Log.Error($"Unhandled exception via {source}. StackTrace: {ex.StackTrace}", ex, source);
            }
            catch
            {
                // ignore secondary failures
            }
        }
    }
}
