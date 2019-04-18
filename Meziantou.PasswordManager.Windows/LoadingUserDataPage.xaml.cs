using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;
using Meziantou.PasswordManager.Windows.Settings;
using System;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for LoadingUserDataPage.xaml
    /// </summary>
    public partial class LoadingUserDataPage : UserControl
    {
        public LoadingUserDataPage()
        {
            InitializeComponent();
            Loaded += LoadingUserDataPage_Loaded;
        }

        private async void LoadingUserDataPage_Loaded(object sender, RoutedEventArgs e)
        {
            var context = PasswordManagerContext.Current;
            var user = context.User;
            if (user.Documents == null)
            {
                var documents = await context.Client.LoadDocumentsAsync();
                user.Documents = new ObservableCollection<Document>(documents);

                if (context.CanCacheData)
                {
                    UserSettings.Current.UserCache = user;
                    UserSettings.Current.Save();
                }
            }

            this.NavigateToMainPage();
        }
    }
}