using System;
using System.IO;
using System.Windows;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    ///     Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            using (var sw = new StringWriter())
            {
                sw.WriteLine("Version: " + Version);
                if (BuildDate != null)
                {
                    sw.WriteLine("Built on: " + BuildDate.Value.ToString("g"));
                }

                if (TimeOfLastUpdateCheck != null)
                {
                    sw.WriteLine("Time of last update check: " + TimeOfLastUpdateCheck.Value.ToString("g"));
                }

                TextBlockDetails.Text = sw.ToString();
            }
        }

        public string Version => App.Version;

        public DateTime? TimeOfLastUpdateCheck => App.TimeOfLastUpdateCheck;

        public DateTime? BuildDate => App.BuildDate;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}