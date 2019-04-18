using System.ComponentModel;
using System.Windows;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.ViewModel;
using Meziantou.PasswordManager.Windows.Utilities;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private readonly ChangePasswordViewModel _viewModel = new ChangePasswordViewModel();

        public ChangePasswordWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsValid)
            {
                _viewModel.ErrorText = ((IDataErrorInfo) _viewModel).Error;
                return;
            }

            try
            {
                _viewModel.CanGenerate = false;

                try
                {
                    await PasswordManagerContext.Current.Client.ChangePasswordAsync(_viewModel.CurrentPassword, _viewModel.NewPassword);
                    Telemetry.TrackEvent("Password changed");
                    Close();
                }
                catch (PasswordManagerException ex)
                {
                    _viewModel.ErrorText = ex.Message;
                }
            }
            finally
            {
                _viewModel.CanGenerate = true;
            }
        }
    }
}