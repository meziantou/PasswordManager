using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for MasterKeyWindow.xaml
    /// </summary>
    public partial class MasterKeyWindow : Window
    {
        private bool _closing = false;

        public MasterKeyWindow(int attempt)
        {
            InitializeComponent();
            Deactivated += MasterKeyWindow_Deactivated;
            Closing += MasterKeyWindow_Closing;

            if (attempt <= 1)
            {
                TextBlockError.Visibility = Visibility.Collapsed;
            }
        }

        private void MasterKeyWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closing = true;
        }

        private void MasterKeyWindow_Deactivated(object sender, EventArgs e)
        {
            if (!_closing)
            {
                App.SetForeground(this);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    TxtMasterKey.Focus();
                }));
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public string GetMasterKey()
        {
            return TxtMasterKey.Password;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            TxtMasterKey.Focus();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.SetVisibility(this);
            while (!_closing && !TxtMasterKey.IsKeyboardFocused)
            {
                await Task.Delay(100);
                App.SetVisibility(this);
            }
        }

        private void TxtMasterKey_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TxtMasterKey.Focus();
        }
    }
}