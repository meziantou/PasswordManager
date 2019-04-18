using System;
using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;
using Meziantou.PasswordManager.Windows.Settings;
using System.Net.Http;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for LoginAutoPage.xaml
    /// </summary>
    public partial class LoginAutoPage : UserControl
    {
        public LoginAutoPage()
        {
            InitializeComponent();
            Loaded += LoginAutoPage_Loaded;
        }

        private async void LoginAutoPage_Loaded(object sender, RoutedEventArgs e)
        {
            var canUseCache = false;
            var context = PasswordManagerContext.Current;
            try
            {
                await context.AutoLoginAsync();
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (HttpRequestException)
            {
                canUseCache = true;
            }

            if (context.IsLoggedIn)
            {
                Telemetry.SetUser(context.User);
                Telemetry.TrackEvent("AutoLogin");
                this.NavigateToMainPage();
            }
            else
            {
                if (canUseCache && UserSettings.Current.UserCache != null)
                {
                    context.User = UserSettings.Current.UserCache;
                    Telemetry.SetUser(context.User);
                    Telemetry.TrackEvent("AutoLogin (cache)");
                    this.NavigateToMainPage();
                }
                else
                {
                    this.NavigateToLoginPage();
                }
            }
        }
    }
}