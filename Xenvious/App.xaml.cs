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
