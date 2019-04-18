using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Meziantou.Framework.Utilities;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Settings;
using System.Reflection;
using System.IO;
using Meziantou.PasswordManager.Windows.Utilities;
using System.Collections.Generic;
using System.Configuration;
using Meziantou.Framework;
using Microsoft.Win32;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if DEBUG
        public const string Name = "Meziantou_PasswordManagerDEBUG";
        public const string MutexName = "{d10fe938-6252-41fb-bfaf-a635dfecc1ee}";
#else
        public const string Name = "Meziantou_PasswordManager";
        public const string MutexName = "{41954a3f-a574-459b-b2df-da0ffb770f85}";
#endif

        private Mutex _instanceMutex;

        internal HttpServer HttpServer { get; private set; }

        public static IList<Action<string[]>> NewInstanceHandler { get; } = new List<Action<string[]>>();

        public static string Version
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();

                return "0.0.0.0";
            }
        }

        public static DateTime? TimeOfLastUpdateCheck
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                    return ApplicationDeployment.CurrentDeployment.TimeOfLastUpdateCheck;

                return null;
            }
        }

        public static DateTime? BuildDate => GetLinkerTimestamp(typeof(AboutWindow).Assembly);

        public static void Restart()
        {
            ProcessStartInfo currentStartInfo = Process.GetCurrentProcess().StartInfo;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                currentStartInfo.FileName = ApplicationDeployment.CurrentDeployment.UpdatedApplicationFullName;
            }
            else
            {
                currentStartInfo.FileName = typeof(App).Assembly.Location;
            }

            currentStartInfo.Arguments = "/Updated";
            Process.Start(currentStartInfo);
            Environment.Exit(0);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ErrorWindow.RegisterErrorHandler();
            PasswordManagerUri.RegisterUriScheme();

            var masterKeyStore = new TemporaryKeyStore
            {
                KeyDuration = TimeSpan.FromSeconds(UserSettings.Current.MasterKeyPersistenceTime),
                ResetTimerOnAccess = UserSettings.Current.MasterKeyResetTimerOnAccess
            };
            PasswordManagerContext.Current.MasterKeyProvider = new MasterKeyProvider();
            PasswordManagerContext.Current.MasterKeyStore = masterKeyStore;
            var apiUrl = ConfigurationManager.AppSettings["Meziantou.PasswordManager.ApiUrl"];
            if (!string.IsNullOrEmpty(apiUrl))
            {
                PasswordManagerContext.Current.Client.ApiUrl = apiUrl;
            }

            if (UserSettings.Current.SingleInstance)
            {
                _instanceMutex = new Mutex(true, MutexName);
                var parser = new CommandLineParser();
                parser.Parse(e.Args);

                if (parser.HasArgument("updated"))
                {
                    if (!_instanceMutex.WaitOne(60000)) // 60s to close the previous instance
                    {
                        MessageBox.Show("Application was unabled to restart", "Password Manager", MessageBoxButton.OK);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    if (!_instanceMutex.WaitOne(0))
                    {
                        // Another instance is already started.
                        // Send a message to show the main window
                        try
                        {
                            var channel = new IpcChannel("Client");
                            ChannelServices.RegisterChannel(channel, false);
                            var app = (SingleInstance)Activator.GetObject(typeof(SingleInstance), $"ipc://{Name}/RemotingServer");

                            if (e.Args.Length == 0)
                            {
                                app.Execute(null);
                            }
                            else
                            {
                                app.Execute(e.Args);
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.ToString());
                        }

                        Environment.Exit(0);
                    }
                }

                try
                {
                    RegisterIpcServer();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }

            HttpServer = new HttpServer();
            if (UserSettings.Current.EnableHttpServer)
            {
                HttpServer.Start();
            }

            SetRunOnStartUp();
            Telemetry.TrackEvent("Application started");
            base.OnStartup(e);
        }

        private void SetRunOnStartUp()
        {
            // https://msdn.microsoft.com/en-us/library/aa376977.aspx
            using (var regKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (UserSettings.Current.RunOnStartUp)
                {
                    var location = Assembly.GetEntryAssembly().Location;
                    regKey.SetValue("Meziantou.PasswordManager", CommandLineBuilder.WindowsQuotedArgument(location) + " /minimize", RegistryValueKind.String);
                }
                else
                {
                    regKey.DeleteValue("Meziantou.PasswordManager");
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (HttpServer != null)
            {
                HttpServer.Dispose();
                HttpServer = null;
            }

            if (_instanceMutex != null)
            {
                _instanceMutex.ReleaseMutex();
                _instanceMutex.Dispose();
            }

            if (!PasswordManagerContext.Current.CanAutoLogin)
            {
                UserSettings.Current.RemoveUserData();
            }

            Telemetry.TrackEvent("Application exited");
            Telemetry.Flush();
            Thread.Sleep(1000);
            base.OnExit(e);
        }

        private static void RegisterIpcServer()
        {
            var channel = new IpcChannel(Name);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(SingleInstance), "RemotingServer", WellKnownObjectMode.Singleton);
        }

        private static DateTime? GetLinkerTimestamp(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            try
            {
                return GetLinkerTimestamp(assembly.Location);
            }
            catch
            {
                return null;
            }
        }

        private static DateTime? GetLinkerTimestamp(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                if (!File.Exists(filePath))
                    return null;

                byte[] buffer = new byte[2048];
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Read(buffer, 0, buffer.Length);
                }

                var int32 = BitConverter.ToInt32(buffer, 60);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(BitConverter.ToInt32(buffer, int32 + 8));
                dateTime = dateTime.ToLocalTime();
                return dateTime;
            }
            catch
            {
                return null;
            }
        }

        private class SingleInstance : MarshalByRefObject
        {
            public void Execute(string[] args)
            {
                Current.Dispatcher.BeginInvoke((Action)SetVisibility, DispatcherPriority.Render);
                foreach (var handler in NewInstanceHandler)
                {
                    handler(args);
                }
            }

            private void SetVisibility()
            {
                var window = Current.MainWindow;
                App.SetVisibility(window);
            }
        }

        internal static void SetVisibility(Window window)
        {
            if (window != null)
            {
                if (!window.IsVisible)
                {
                    window.Show();
                }

                SetForeground(window);
            }
        }

        internal static void SetForeground(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                Restore(window);
            }

            window.Activate();
            window.Focus();
            SetForegroundWindow(new WindowInteropHelper(window).Handle);
        }

        private static void Restore(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            const uint SW_RESTORE = 0x09;
            if (window.WindowState == WindowState.Minimized)
            {
                ShowWindow(new WindowInteropHelper(window).Handle, SW_RESTORE);
            }
        }

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint msg);
    }
}