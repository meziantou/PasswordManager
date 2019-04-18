using System.Windows;
using Meziantou.PasswordManager.Client;

namespace Meziantou.PasswordManager.Windows
{
    public class MasterKeyProvider : IMasterKeyProvider
    {
        public string GetMasterKey(int attempt)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var window = new MasterKeyWindow(attempt);
                window.Owner = Application.Current.MainWindow;
                if (window.ShowDialog() == true)
                {
                    return window.GetMasterKey();
                }

                return null;
            });
        }
    }
}