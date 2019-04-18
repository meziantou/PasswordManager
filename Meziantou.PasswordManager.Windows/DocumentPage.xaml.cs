using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Utilities;

namespace Meziantou.PasswordManager.Windows
{
    public partial class DocumentPage : UserControl
    {
        public Document Document { get; }

        public DocumentPage(Document document)
        {
            InitializeComponent();

            if (document == null)
            {
                document = new Document();
                document.User = PasswordManagerContext.Current.User;
            }

            Document = document;
            DataContext = Document;
        }

        private void CopyToClipboardButton_OnClick(object sender, RoutedEventArgs e)
        {

            if (((FrameworkElement)sender).DataContext is Field field)
            {
                ClipboardUtilities.CopyFieldValueToClipboard(field);
            }
        }

        private void OpenWebsiteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Field field)
            {
                try
                {
                    var str = field.GetValueAsString();
                    Process.Start(str);
                }
                catch (PasswordManagerException ex)
                {
                    if (ex.Code == ErrorCode.InvalidMasterKey)
                        return;

                    throw;
                }
            }
        }
    }
}