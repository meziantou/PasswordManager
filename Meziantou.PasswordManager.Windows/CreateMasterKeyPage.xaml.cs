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
    ///     Interaction logic for CreateUserKeyPage.xaml
    /// </summary>
    public partial class CreateMasterKeyPage : UserControl
    {
        private readonly CreateMasterKeyViewModel _viewModel = new CreateMasterKeyViewModel();

        public CreateMasterKeyPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
            Loaded += CreateUserKeyPage_Loaded;
        }

        private void CreateUserKeyPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtMasterKey.Focus();
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
                var masterKey = _viewModel.MasterKey;
                _viewModel.CanGenerate = false;

                var context = PasswordManagerContext.Current;
                var user = context.User;
                await Task.Run(() =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    user.GenerateKey(masterKey);
                });

                user = await context.Client.SetUserKeyAsync(user.PublicKey, user.PrivateKey);
                Telemetry.TrackEvent("MasterKey created");
                this.NavigateToMainPage();
            }
            finally
            {
                _viewModel.CanGenerate = true;
            }
        }
    }
}