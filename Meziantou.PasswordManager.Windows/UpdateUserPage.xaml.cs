using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    /// Interaction logic for UpdateUserPage.xaml
    /// </summary>
    public partial class UpdateUserPage : UserControl
    {
        public UpdateUserPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var context = PasswordManagerContext.Current;
            var user = context.User;

            try
            {
                UpdateButton.IsEnabled = false;
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        var masterKey = context.GetMasterKey(i);
                        await context.Client.UpdateUserAsync(user, masterKey);
                        this.NavigateToMainPage();
                        return;
                    }
                    catch(PasswordManagerException ex) when (ex.Code == ErrorCode.InvalidMasterKey)
                    {
                        context.ClearMasterKey();
                    }
                }
            }
            finally
            {
                UpdateButton.IsEnabled = true;
            }
        }
    }
}
