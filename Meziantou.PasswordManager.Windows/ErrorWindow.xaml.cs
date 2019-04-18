using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Meziantou.Framework.Utilities;
using Meziantou.PasswordManager.Windows.Utilities;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
            ShowIcon();
        }

        public bool ShowDetails
        {
            get => GridDetails.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    ButtonShowDetails.Visibility = Visibility.Collapsed;
                    ButtonHideDetails.Visibility = Visibility.Visible;
                    GridDetails.Visibility = Visibility.Visible;
                }
                else
                {
                    ButtonShowDetails.Visibility = Visibility.Visible;
                    ButtonHideDetails.Visibility = Visibility.Collapsed;
                    GridDetails.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowIcon()
        {
            var error = SystemIcons.Error;
            var image = Imaging.CreateBitmapSourceFromHIcon(error.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            ImageIcon.Source = image;
        }

        public static void RegisterErrorHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            var threadDispatcher = Dispatcher.CurrentDispatcher;
            var applicationDispatcher = Application.Current.Dispatcher;
            threadDispatcher.UnhandledException += DispatcherUnhandledException;
            if (threadDispatcher != applicationDispatcher)
            {
                Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
            }
        }

        public static void ShowError(Exception ex, bool canContinue)
        {
            ShowError(ex.Message, ex.ToString(includeInnerException: true), canContinue);
        }

        public static void ShowError(string message, string details, bool canContinue)
        {
            if (!CanDispatch())
            {
                // TODO log and quit
                return;
            }

            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                // Thread must be STA...
                var t = new Thread(o => { ShowWindow(message, details, canContinue); });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                return;
            }

            ShowWindow(message, details, canContinue);
        }

        private static bool CanDispatch()
        {
            var app = Application.Current;
            if (app == null)
                return false;

            return !app.Dispatcher.HasShutdownFinished && !app.Dispatcher.HasShutdownStarted;
        }

        private static void ShowWindow(string message, string details, bool canContinue)
        {
            var window = new ErrorWindow();
            window.TextBlockMessage.Text = message;
            window.TextBlockDetails.Text = details;
            window.ShowDialog();
        }

        private static void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Telemetry.TrackException(e.Exception);
            ShowError(e.Exception, true);
            e.Handled = true;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Telemetry.TrackException(e.Exception);
            ShowError(e.Exception, true);
            e.SetObserved();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Telemetry.TrackException(ex);
            ShowError(ex, !e.IsTerminating);
        }

        private void ButtonShowDetails_OnClick(object sender, RoutedEventArgs e)
        {
            ShowDetails = true;
        }

        private void ButtonHideDetails_OnClick(object sender, RoutedEventArgs e)
        {
            ShowDetails = false;
        }

        private void ButtonQuit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}