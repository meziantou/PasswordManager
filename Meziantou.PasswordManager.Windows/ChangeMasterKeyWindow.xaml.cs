using System.ComponentModel;
using System.Windows;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.ViewModel;
using Meziantou.PasswordManager.Windows.Utilities;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for ChangeMasterKeyWindow.xaml
    /// </summary>
    public partial class ChangeMasterKeyWindow : Window
    {
        private readonly ChangeMasterKeyViewModel _viewModel = new ChangeMasterKeyViewModel();

        public ChangeMasterKeyWindow()
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
                    await PasswordManagerContext.Current.User.ChangeMasterKeyAsync(_viewModel.CurrentMasterKey, _viewModel.MasterKey);
                    Telemetry.TrackEvent("MasterKey changed");
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