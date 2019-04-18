using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Settings;
using Meziantou.PasswordManager.Windows.Utilities;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Meziantou.Framework.Utilities;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PasswordManagerContext _passwordManagerContext = PasswordManagerContext.Current;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            Title += " - DEBUG";
#endif

            UserSettings.Current.State.RestoreWindowState(this);

            this.NavigateToLoginPage();
            Menu.DataContext = _passwordManagerContext;
            
            EnableHttpServer.IsChecked = UserSettings.Current.EnableHttpServer;
        }

        public void Navigate(object content)
        {
            MainFrame.Content = content;
        }

        private void CloseCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LogOutCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _passwordManagerContext.LogOut();
            UserSettings.Current.RemoveUserData();

            Telemetry.TrackEvent("LogOut");
            Telemetry.SetUser(null);
            this.NavigateToLoginPage();
        }

        private void LogOutCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _passwordManagerContext.IsLoggedIn;
        }

        private void MenuItemAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }
             
        private void RefreshCommand_OnExecuted(object sender, RoutedEventArgs e)
        {
            _passwordManagerContext.User.Documents = null;
            this.NavigateToLoadingUserDataPage();
        }

        private void RefreshCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _passwordManagerContext.IsLoggedIn && _passwordManagerContext.User.Documents != null;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // Save state
            var settings = UserSettings.Current;
            settings.State.SaveWindowLocation(this);
            settings.Save();
        }

        private void ChangeMasterKeyCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new ChangeMasterKeyWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ChangeMasterKeyCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _passwordManagerContext.IsLoggedIn && _passwordManagerContext.User.PrivateKey != null;
        }

        private void ChangePasswordCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _passwordManagerContext.IsLoggedIn;
        }

        private void ChangePasswordCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new ChangePasswordWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ExportDocumentsCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _passwordManagerContext.IsLoggedIn;
        }

        private void ExportDocumentsCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "json";
            dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            dialog.AddExtension = true;
            if (dialog.ShowDialog() == true)
            {
                var json = _passwordManagerContext.User.ExportDocumentToJson(out IList<Document> errors);
                File.WriteAllText(dialog.FileName, json, Encoding.UTF8);

                if (errors != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Cannot export documents: ");
                    foreach (var doc in errors)
                    {
                        sb.AppendLine("- " + doc.FinalDisplayName);
                    }

                    MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TestPasswordsCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _passwordManagerContext.IsLoggedIn;
        }

        private async void TestPasswordsCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = "txt";
            dialog.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.AddExtension = true;
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                if (dialog.SafeFileNames.Length == 0)
                    return;

                var items = _passwordManagerContext.User.Documents
                    .SelectMany(doc => doc.Fields)
                    .Where(f => f.Type == FieldValueType.Password)
                    .Select(f => Tuple.Create(ToSha1(f.GetValueAsString()), f.Document))
                    .ToList();

                var matches = await FindMatches(items, dialog.FileNames);
                if (matches.Any())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("WARNING: ");
                    foreach (var doc in matches)
                    {
                        sb.AppendLine("- " + doc.FinalDisplayName);
                    }

                    MessageBox.Show(sb.ToString(), "You have been pwned", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Your password are not in the database", "You are safe", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            async Task<List<Document>> FindMatches(List<Tuple<string, Document>> items, IEnumerable<string> files)
            {
                var matches = new List<Document>();
                foreach (var file in dialog.FileNames)
                {
                    using (var fs = File.OpenRead(file))
                    using (var sr = new StreamReader(fs))
                    {
                        string line;
                        while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                        {
                            for (var i = items.Count - 1; i >= 0; i--)
                            {
                                var item = items[i];
                                if (string.Equals(item.Item1, line, StringComparison.OrdinalIgnoreCase))
                                {
                                    matches.Add(item.Item2);
                                    items.RemoveAt(i);

                                    if (items.Count == 0)
                                        return matches;
                                }
                            }
                        }
                    }
                }

                return matches;
            }
        }

        private async void TestPasswordsOnlineCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var items = _passwordManagerContext.User.Documents
                .SelectMany(doc => doc.Fields)
                .Where(f => f.Type == FieldValueType.Password)
                .Select(f => Tuple.Create(ToSha1(f.GetValueAsString()), f.Document))
                .GroupBy(f => f.Item1)
                .ToList();

            if (items.Count == 0)
                return;

            var matches = new List<Document>();
            using (var httpClient = new HttpClient(new HaveIBeenPwnedHttpClientHandler(), disposeHandler: true))
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "PasswordManager");
                foreach (var item in items)
                {
                    using (var response = await httpClient.GetAsync("https://haveibeenpwned.com/api/v2/pwnedpassword/" + item.Key)) // sha1 is already encoded
                    {
                        if (response == null)
                            continue;

                        switch (response.StatusCode)
                        {
                            case System.Net.HttpStatusCode.NotFound:
                                break;

                            case System.Net.HttpStatusCode.OK:
                                matches.AddRange(item.Select(_ => _.Item2));
                                break;

                            default:
                                break; // TODO handle error
                        }
                    }
                }
            }

            if (matches.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("WARNING: ");
                foreach (var doc in matches)
                {
                    sb.AppendLine("- " + doc.FinalDisplayName);
                }

                MessageBox.Show(sb.ToString(), "You have been pwned", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Your password are not in the database", "You are safe", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static string ToSha1(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(bytes);
                return hash.ToHexa(HexaOptions.LowerCase);
            }
        }

        private void EnableHttpServer_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.Current.EnableHttpServer = EnableHttpServer.IsChecked;
            UserSettings.Current.Save();

            var server = (Application.Current as App)?.HttpServer;
            if (UserSettings.Current.EnableHttpServer)
            {
                server.Start();
            }
            else
            {
                server.Stop();
            }
        }

        private void OpenMainWindowCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenMainWindowCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            App.SetVisibility(this);
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized && UserSettings.Current.MinimizeToSystemTray)
            {
                Hide();
            }
        }

        private void TaskbarExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TaskbarOpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            App.SetVisibility(this);
        }
    }
}