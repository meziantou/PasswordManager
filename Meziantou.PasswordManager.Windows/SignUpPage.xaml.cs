using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;
using Meziantou.PasswordManager.Windows.ViewModel;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : UserControl
    {
        private readonly SignUpViewModel _viewModel = new SignUpViewModel();

        public SignUpPage()
        {
            InitializeComponent();
            DataContext = _viewModel;

            Loaded += SignUpPage_Loaded;
        }

        private void SignUpPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtUsername.Focus();
        }

        private async void ButtonSignUp_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsValid)
            {
                _viewModel.ErrorText = ((IDataErrorInfo) _viewModel).Error;
                return;
            }

            try
            {
                _viewModel.CanSignUp = false;

                var username = _viewModel.Username;
                var password = _viewModel.Password;

                var context = PasswordManagerContext.Current;
                var client = context.Client;
                try
                {
                    var user = await client.SignUpAsync(username, password);
                    context.User = user;
                }
                catch (PasswordManagerException ex)
                {
                    _viewModel.ErrorText = ex.Message;
                    return;
                }

                client.SetCredential(username, password);
                Telemetry.SetUser(context.User);
                Telemetry.TrackEvent("Signup");
                this.NavigateToMainPage();
            }
            finally
            {
                _viewModel.CanSignUp = true;
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            this.NavigateToLoginPage();
        }
    }
}