using System.Windows;
using Meziantou.PasswordManager.Windows.ViewModel;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    /// Interaction logic for CreateFieldWindow.xaml
    /// </summary>
    public partial class CreateFieldWindow : Window
    {
        public CreateFieldWindow()
        {
            InitializeComponent();
        }

        public EditableField EditableField
        {
            get => DataContext as EditableField;
            set => DataContext = value;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
