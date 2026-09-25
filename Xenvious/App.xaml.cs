using System;
using System.Collections.Generic;
using System.Windows;
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
