using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Settings;
using Meziantou.PasswordManager.Windows.Utilities;
using Meziantou.PasswordManager.Windows.ViewModel;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl
    {
        private static bool _firstRun = true;
        private DebounceDispatcher _debounceDispatcher = new DebounceDispatcher();

        public MainPage()
        {
            InitializeComponent();
            RestoreState();

            Loaded += OnLoaded;
            Unloaded += MainPage_Unloaded;

            var width = DependencyPropertyDescriptor.FromProperty(ColumnDefinition.WidthProperty, typeof(ItemsControl));
            width.AddValueChanged(ColumnDefinitionLeft, GridSplitterChanged);
            App.NewInstanceHandler.Add(HandlerUrl);
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.NewInstanceHandler.Remove(HandlerUrl);
        }

        private void HandlerUrl(string[] args)
        {
            if (args == null)
                return;

            foreach (var arg in args)
            {
                var uri = PasswordManagerUri.Parse(arg);
                if (uri == null)
                    continue;

                if (!uri.IsSearch)
                    continue;

                var url = uri.GetSearchUrl();
                if (url != null)
                {
                    TxtSearch.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        TxtSearch.Text = "url:" + url;
                        if (uri.CopyToClipboard(true))
                        {
                            var selectedDocument = GetSelectedDocument();
                            if (selectedDocument != null && DocumentsFilter(selectedDocument))
                            {
                                var passwords = selectedDocument.Fields.Where(field => field.Type == FieldValueType.Password).ToList();
                                if (passwords.Count == 1)
                                {
                                    ClipboardUtilities.CopyFieldValueToClipboard(passwords[0]);
                                }
                            }
                        }
                    }));
                }
            }
        }

        private void RestoreState()
        {
            if (UserSettings.Current.State.GridSplitterPosition != null)
            {
                for (var i = 0; i < UserSettings.Current.State.GridSplitterPosition.Count && i < MainGrid.ColumnDefinitions.Count; i++)
                {
                    var gridLenthState = UserSettings.Current.State.GridSplitterPosition[i];
                    MainGrid.ColumnDefinitions[i].Width = new GridLength(gridLenthState.Value, gridLenthState.GridUnitType);
                }
            }
        }

        private void GridSplitterChanged(object sender, EventArgs e)
        {
            var list = new List<GridLenthState>();
            for (var i = 0; i < MainGrid.ColumnDefinitions.Count; i++)
            {
                var columnDefinition = MainGrid.ColumnDefinitions[i];
                list.Add(new GridLenthState { Value = columnDefinition.Width.Value, GridUnitType = columnDefinition.Width.GridUnitType });
            }

            UserSettings.Current.State.GridSplitterPosition = list;
            UserSettings.Current.Save();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = PasswordManagerContext.Current;
            if (context.User == null)
            {
                this.NavigateToLoginPage();
                return;
            }

            if (context.User.PublicKey == null)
            {
                this.NavigateToCreateUserKeyPage();
                return;
            }

            if (context.User.Documents == null)
            {
                this.NavigateToLoadingUserDataPage();
                return;
            }

            if (!context.User.IsUpToDate())
            {
                this.NavigateToUpdateUserPage();
                return;
            }

            if (_firstRun)
            {
                if (Environment.GetCommandLineArgs().ContainsIgnoreCase("/minimize"))
                {
                    var window = Window.GetWindow(this);
                    if (window != null)
                    {
                        window.WindowState = WindowState.Minimized;
                        if (UserSettings.Current.MinimizeToSystemTray)
                        {
                            window.Hide();
                        }
                    }
                }

                HandlerUrl(Environment.GetCommandLineArgs());
                _firstRun = false;
            }

            CreateTreeviewItems();
            TxtSearch.Focus();

            RegisterOnDocumentCollectionChanged();
        }

        private void RegisterOnDocumentCollectionChanged()
        {
            if (PasswordManagerContext.Current.User.Documents is INotifyCollectionChanged documents)
            {
                documents.CollectionChanged += Documents_CollectionChanged;
            }
        }

        private void Documents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CreateTreeviewItems();
        }

        private void CreateTreeviewItems()
        {
            if (PasswordManagerContext.Current.User.Documents == null)
                return;

            var documents = PasswordManagerContext.Current.User.Documents.Where(DocumentsFilter).ToList();

            TreeViewDocuments.Items.Clear();

            var root = DocumentGroup.Group(documents);
            CreateTreeviewItem(TreeViewDocuments.Items, root);

            if (TreeViewDocuments.SelectedItem == null)
            {
                SelectDocument(UserSettings.Current.State.LastSelectedDocumentId);
            }

            if (TreeViewDocuments.SelectedItem == null)
            {
                SelectFirstDocument();
            }
        }

        private void SelectFirstDocument()
        {
            SelectDocument((string)null);
        }

        private void SelectDocument(Document document)
        {
            SelectDocument(document?.Id);
        }

        private bool SelectDocument(string id)
        {
            return SelectDocument(TreeViewDocuments.Items, id);
        }

        private bool SelectDocument(ItemCollection items, string id)
        {
            foreach (var item in items)
            {
                if (SelectDocument(item, id))
                    return true;
            }

            return false;
        }

        private bool SelectDocument(object item, string id)
        {
            if (item is TreeViewItem treeViewItem)
            {
                if (treeViewItem.Tag is Document document)
                {
                    if (id == null || document.Id == id)
                    {
                        treeViewItem.IsSelected = true;
                        return true;
                    }
                }

                return SelectDocument(treeViewItem.Items, id);
            }

            return false;
        }

        private void CreateTreeviewItem(ItemCollection parent, DocumentGroup group)
        {
            var isSearch = !string.IsNullOrEmpty(TxtSearch.Text);
            if (!group.IsRoot)
            {
                var groupItem = new TreeViewItem
                {
                    Header = group.Name,
                    IsExpanded = isSearch
                };
                parent.Add(groupItem);
                if (!isSearch)
                {
                    UserSettings.Current.State.DocumentsTreeViewState.RestoreState(groupItem);
                }

                parent = groupItem.Items;
            }

            foreach (var childGroup in group.Groups)
            {
                CreateTreeviewItem(parent, childGroup);
            }

            foreach (var document in group.Documents)
            {
                var item = CreateTreeViewItem(document);
                parent.Add(item);
            }
        }

        private static TreeViewItem CreateTreeViewItem(Document groupDocument)
        {
            var documentItem = new TreeViewItem
            {
                Tag = groupDocument,
                Header = groupDocument.FinalDisplayName
            };
            return documentItem;
        }

        private bool DocumentsFilter(Document document)
        {
            if (document == null)
                return false;

            var filterText = TxtSearch.Text?.Trim();
            if (!string.IsNullOrEmpty(filterText))
            {
                if (filterText.StartsWith("url:", StringComparison.OrdinalIgnoreCase))
                {
                    var searchUrl = filterText.Substring("url:".Length);
                    return document.MatchSearchUrl(searchUrl);
                }
                else
                {
                    string name = null;
                    if (!string.IsNullOrEmpty(document.UserDisplayName))
                    {
                        name = document.UserDisplayName;
                    }
                    else if (!string.IsNullOrEmpty(document.DisplayName))
                    {
                        name = document.DisplayName;
                    }

                    if (name != null && name.ContainsIgnoreCase(filterText))
                        return true;

                    foreach (var field in document.Fields)
                    {
                        if (!field.IsEncrypted && field.DisplayValue.ContainsIgnoreCase(filterText))
                            return true;
                    }

                    // The following should not be necessary (FinalDisplayName is computed from fields)
                    //if (document.FinalDisplayName.ContainsIgnoreCase(filterText))
                    //    return true;

                    return false;
                }
            }

            return true;
        }

        private void ButtonAddNewDocument_Click(object sender, RoutedEventArgs e)
        {
            EditDocument(null);
        }

        private async void MenuItemDelete_OnClick(object sender, RoutedEventArgs e)
        {
            await RemoveSelectedDocument();
        }

        private async Task RemoveSelectedDocument()
        {
            var selectedItem = GetSelectedDocument();
            if (selectedItem == null)
                return;

            var result = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = PasswordManagerContext.Current;
                    await context.Client.DeleteDocumentAsync(selectedItem);
                    DocumentFrame.Content = null;
                    context.User.Documents.Remove(selectedItem);

                    if (context.CanCacheData)
                    {
                        UserSettings.Current.UserCache = context.User;
                        UserSettings.Current.Save();
                    }
                }
                catch (PasswordManagerException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void MenuItemShare_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = GetSelectedDocument();
            if (selectedItem == null)
                return;

            var shareDocumentWindow = new ShareDocumentWindow(selectedItem)
            {
                Owner = Window.GetWindow(this)
            };
            shareDocumentWindow.ShowDialog();
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _debounceDispatcher.Debounce(300, _ =>
            {
                CreateTreeviewItems();
            });
        }

        private void TreeView_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var element = e.Source as TreeViewItem;
            if (element == null)
            {
                e.Handled = true;
                return;
            }

            var document = element.Tag as Document;
            if (document == null)
            {
                e.Handled = true;
                return;
            }

            element.IsSelected = true;

            ContextMenuShareWith.IsEnabled = !document.IsSharedBySomeone;
            ContextMenuEdit.IsEnabled = true;
            ContextMenuDelete.IsEnabled = !document.IsSharedBySomeone;
        }

        private void CommandFind_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            TxtSearch.Focus();
        }

        private void CommandDelete_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var selectedItem = GetSelectedDocument();
            e.CanExecute = selectedItem?.IsSharedBySomeone == false;
        }

        private async void CommandDelete_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            await RemoveSelectedDocument();
        }

        private void MenuItemEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = GetSelectedDocument();
            if (selectedItem == null)
                return;

            EditDocument(selectedItem);
        }

        private Document GetSelectedDocument()
        {
            var item = TreeViewDocuments.SelectedItem;
            if (item is TreeViewItem tvi)
            {
                return tvi.Tag as Document;
            }

            return item as Document;
        }

        private void EditDocument(Document document)
        {
            EditableDocument editableDocument;
            if (document == null)
            {
                editableDocument = EditableDocument.CreateDefaultWebDocument();
            }
            else
            {
                try
                {
                    editableDocument = new EditableDocument(document);
                    //editableDocument.AddMissingDefaultWebField();
                }
                catch (PasswordManagerException ex)
                {
                    if (ex.Code == ErrorCode.InvalidMasterKey)
                        return;

                    throw;
                }
            }

            var documentFrameContent = new EditDocumentPage(editableDocument);
            documentFrameContent.DocumentSaved += DocumentFrameContent_DocumentSaved;
            DocumentFrame.Content = documentFrameContent;
        }

        private void DocumentFrameContent_DocumentSaved(object sender, DocumentSaveEventArgs e)
        {
            SelectDocument(e.Document);
            ((EditDocumentPage)sender).DocumentSaved -= DocumentFrameContent_DocumentSaved;
        }

        private void TxtSearch_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                TxtSearch.Text = "";
            }
        }

        private void TreeViewDocuments_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var doc = GetSelectedDocument();
            if (doc != null)
            {
                DocumentFrame.Content = new DocumentPage(doc);
                UserSettings.Current.State.LastSelectedDocumentId = doc.Id;
                UserSettings.Current.Save();
            }
        }

        private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSearch.Text))
            {
                UserSettings.Current.State.DocumentsTreeViewState.SaveState(sender as TreeViewItem);
                UserSettings.Current.Save();
            }
        }

        private void OnTreeViewItemCollapsed(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSearch.Text))
            {
                UserSettings.Current.State.DocumentsTreeViewState.SaveState(sender as TreeViewItem);
                UserSettings.Current.Save();
            }
        }
    }
}