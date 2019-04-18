using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.ViewModel;
using Meziantou.PasswordManager.Windows.Utilities;
using System.Globalization;
using Meziantou.PasswordManager.Windows.Settings;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for EditDocumentPage.xaml
    /// </summary>
    public partial class EditDocumentPage : UserControl
    {
        private readonly EditableDocument _editableDocument;

        public EditDocumentPage() : this(null)
        {
        }

        public EditDocumentPage(EditableDocument editableDocument)
        {
            _editableDocument = editableDocument;

            InitializeComponent();
            Loaded += EditDocumentPage_Loaded;
            DataContext = editableDocument;
        }

        public event EventHandler<DocumentSaveEventArgs> DocumentSaved;

        private void EditDocumentPage_Loaded(object sender, RoutedEventArgs e)
        {
            TxtDisplayName.Focus();
        }

        private void ButtonGeneratePassword_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new PasswordGeneratorWindow()
            {
                Owner = Window.GetWindow(this)
            };
            if (window.ShowDialog() == true)
            {
                if ((sender as FrameworkElement)?.DataContext is EditableField field)
                {
                    field.ValueString = window.Password;
                }
            }
        }

        private async void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonSave.IsEnabled = false;

                var document = _editableDocument.Document ?? new Document() { User = PasswordManagerContext.Current.User };

                if (document.IsSharedBySomeone)
                {
                    document.UserDisplayName = _editableDocument.DisplayName;
                    document.UserTags = _editableDocument.Tags;
                }
                else
                {
                    document.DisplayName = _editableDocument.DisplayName;
                    document.Tags = _editableDocument.Tags;
                }

                var canEditFields = _editableDocument.CanEditFields;
                if (canEditFields)
                {
                    document.Fields.Clear();

                    // Get all public keys
                    var publicKeys = await PasswordManagerContext.Current.Client.GetUserPublicKeysAsync(document);

                    // Recreate document
                    foreach (var field in _editableDocument.Fields)
                    {
                        if (string.IsNullOrEmpty(field.ValueString))
                            continue;

                        if ((field.Options & FieldOptions.Encrypted) == FieldOptions.Encrypted)
                        {
                            document.AddEncryptedField(field.Name, field.ValueString, field.ValueType, field.Selector, publicKeys);
                        }
                        else
                        {
                            document.AddField(field.Name, field.ValueString, field.ValueType, field.Selector);
                        }
                    }
                }

                var context = PasswordManagerContext.Current;
                var newDocument = await context.Client.SaveDocumentAsync(document);
                context.User.Documents.Remove(document);
                context.User.Documents.Add(newDocument);

                OnDocumentSaved(new DocumentSaveEventArgs(newDocument));

                var properties = new Dictionary<string, string>
                {
                    { "DocumentId", newDocument.Id.ToString(CultureInfo.InvariantCulture) },
                    { "CanEditFields", canEditFields ? "true" : "false" }
                };
                var metrics = new Dictionary<string, double>
                {
                    { "SharedWithCount", newDocument.SharedWith?.Count ?? 0 },
                    { "TagCount", newDocument.FinalTagList.Count() },
                    { "FieldCount", newDocument.Fields.Count() }
                };
                foreach (FieldValueType value in Enum.GetValues(typeof(FieldValueType)))
                {
                    metrics.Add("FieldValueType " + value, newDocument.Fields.Count(field => field.Type == value));
                }
                foreach (FieldOptions value in Enum.GetValues(typeof(FieldOptions)))
                {
                    metrics.Add("FieldOptions " + value, newDocument.Fields.Count(field => (value == 0) ? (field.Options == value) : ((field.Options & value) == value)));
                }

                Telemetry.TrackEvent("Document saved", properties, metrics);
                
                if (context.CanCacheData)
                {
                    UserSettings.Current.UserCache = context.User;
                    UserSettings.Current.Save();
                }
            }
            finally
            {
                ButtonSave.IsEnabled = true;
            }
        }

        protected virtual void OnDocumentSaved(DocumentSaveEventArgs e)
        {
            DocumentSaved?.Invoke(this, e);
        }

        private void ButtonAddField_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement item)
            {
                item.ContextMenu.IsOpen = true;
            }
        }

        private void ButtonAddFieldType_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            if (item == null)
                return;

            if (!(item.Tag is CreateFieldType))
                return;

            var type = (CreateFieldType)item.Tag;
            var field = EditableDocument.CreateField(type);

            var window = new CreateFieldWindow();
            window.Owner = Window.GetWindow(this);
            window.EditableField = field;
            if (window.ShowDialog() == true && !string.IsNullOrWhiteSpace(field.Name))
            {
                _editableDocument.Fields.Add(field);
            }
        }

        private void EmailAddressComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            if (itemsControl == null)
                return;

            itemsControl.ItemsSource = GetFieldValuesByType(FieldValueType.EmailAddress);
        }

        private void LoginComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            if (itemsControl == null)
                return;

            itemsControl.ItemsSource = GetFieldValuesByType(FieldValueType.Username);
        }

        private static List<string> GetFieldValuesByType(FieldValueType fieldType)
        {
            return PasswordManagerContext.Current.User?.Documents?
                .SelectMany(doc => doc.Fields)
                .Where(f => !f.IsEncrypted && f.Type == fieldType)
                .Select(f => f.GetValueAsString())
                .Distinct()
                .OrderBy(_ => _)
                .ToList();
        }

        private void MoveFieldDownButton_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            if (item == null)
                return;

            if (item.DataContext is EditableField editableField)
            {
                var fields = _editableDocument.Fields;
                var index = fields.IndexOf(editableField);
                if (index >= 0 && index < fields.Count - 1)
                {
                    fields.Move(index, index + 1);
                }
            }
        }

        private void MoveFieldUpButton_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            if (item == null)
                return;

            if (item.DataContext is EditableField editableField)
            {
                var fields = _editableDocument.Fields;
                var index = fields.IndexOf(editableField);
                if (index > 0)
                {
                    fields.Move(index, index - 1);
                }
            }
        }

        private void RemoveFieldButton_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            if (item == null)
                return;

            if (item.DataContext is EditableField editableField)
            {
                var fields = _editableDocument.Fields;
                fields.Remove(editableField);
            }
        }
    }
}