using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;
using Meziantou.PasswordManager.Windows.ViewModel;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
        private readonly LoginViewModel _viewModel = new LoginViewModel();

        public LoginPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
            Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            var context = PasswordManagerContext.Current;
            if (context.CanAutoLogin)
            {
                this.NavigateToAutoLoginPage();
            }

            TxtUsername.Focus();
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            await Login(_viewModel.Username, _viewModel.Password, _viewModel.RememberMe);
        }

        private async Task Login(string username, string password, bool rememberMe)
        {
            if (!_viewModel.IsValid)
            {
                _viewModel.ErrorText = ((IDataErrorInfo) _viewModel).Error;
                return;
            }
            try
            {
                _viewModel.CanLogIn = false;

                try
                {
                    var context = PasswordManagerContext.Current;
                    await context.LoginAsync(username, password, rememberMe);
                    if (context.IsLoggedIn)
                    {
                        Telemetry.SetUser(context.User);
                        Telemetry.TrackEvent("Login");
                        this.NavigateToMainPage();
                        return;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }

                _viewModel.ErrorText = "Invalid username or password";
            }
            finally
            {
                _viewModel.CanLogIn = true;
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            this.NavigateToSignUpPage();
        }
    }
}