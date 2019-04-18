using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;
using Meziantou.PasswordManager.Windows.ViewModel;
using System.Collections.Generic;
using Meziantou.PasswordManager.Windows.Settings;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for MasterKeyWindow.xaml
    /// </summary>
    public partial class ShareDocumentWindow : Window
    {
        private readonly ShareDocumentViewModel _viewModel = new ShareDocumentViewModel();

        public ShareDocumentWindow(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            InitializeComponent();
            _viewModel.Document = document;
            DataContext = _viewModel;
            Loaded += ShareDocumentWindow_Loaded;
        }

        private void ShareDocumentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) CbxUsername.Template.FindName("PART_EditableTextBox", CbxUsername);
            if (tb != null)
            {
                tb.Focus();
            }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.CanShare = false;

                if (!_viewModel.IsValid)
                {
                    _viewModel.ErrorText = ((IDataErrorInfo) _viewModel).Error;
                    return;
                }

                var username = _viewModel.Username;
                var document = _viewModel.Document;
                try
                {
                    var context = PasswordManagerContext.Current;
                    var newDocument = await context.Client.ShareDocumentAsync(document, username);
                    if (newDocument.SharedWith == null)
                    {
                        newDocument.SharedWith = new ObservableCollection<string>();
                    }

                    context.User.Documents.Remove(document);
                    context.User.Documents.Add(newDocument);
                                        
                    var properties = new Dictionary<string, string>();
                    properties.Add("DocumentId", document.Id);
                    var metrics = new Dictionary<string, double>();
                    metrics.Add("SharedWith", document.SharedWith.Count);
                    Telemetry.TrackEvent("Document shared", properties, metrics);

                    if (context.CanCacheData)
                    {
                        UserSettings.Current.UserCache = context.User;
                        UserSettings.Current.Save();
                    }

                    DialogResult = true;
                    Close();
                }
                catch (PasswordManagerException ex)
                {
                    if (ex.Code == ErrorCode.InvalidMasterKey)
                    {
                        _viewModel.ErrorText = "You need to provide the master key";
                    }
                    else
                    {
                        _viewModel.ErrorText = ex.Message;
                    }
                }
            }
            finally
            {
                _viewModel.CanShare = true;
            }
        }
    }
}