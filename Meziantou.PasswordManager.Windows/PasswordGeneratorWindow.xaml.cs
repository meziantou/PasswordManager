using System;
using System.Windows;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Settings;
using Meziantou.PasswordManager.Windows.Utilities;
using System.Collections.Generic;

namespace Meziantou.PasswordManager.Windows
{
    /// <summary>
    /// Interaction logic for PasswordGeneratorWindow.xaml
    /// </summary>
    public partial class PasswordGeneratorWindow : Window
    {
        private readonly PasswordGenerator _passwordGenerator = new PasswordGenerator();

        public string Password => TbxPassword.Text;

        public PasswordGeneratorWindow()
        {
            var settings = UserSettings.Current.PasswordGenerator;
            if (settings != null)
            {
                _passwordGenerator.Length = settings.Length;
                _passwordGenerator.LowercaseLetters = settings.LowercaseLetters;
                _passwordGenerator.LowercaseLettersWithAccentMark = settings.LowercaseLettersWithAccentMark;
                _passwordGenerator.UppercaseLetters = settings.UppercaseLetters;
                _passwordGenerator.UppercaseLettersWithAccentMark = settings.UppercaseLettersWithAccentMark;
                _passwordGenerator.Symbols = settings.Symbols;
                _passwordGenerator.Numbers = settings.Numbers;
            }

            InitializeComponent();
            DataContext = _passwordGenerator;
            GeneratePassword();
        }

        private void GeneratePassword()
        {
            TbxPassword.Text = _passwordGenerator.Generate();

            var settings = new PasswordGeneratorSettings
            {
                Length = _passwordGenerator.Length,
                LowercaseLetters = _passwordGenerator.LowercaseLetters,
                LowercaseLettersWithAccentMark = _passwordGenerator.LowercaseLettersWithAccentMark,
                UppercaseLetters = _passwordGenerator.UppercaseLetters,
                UppercaseLettersWithAccentMark = _passwordGenerator.UppercaseLettersWithAccentMark,
                Symbols = _passwordGenerator.Symbols,
                Numbers = _passwordGenerator.Numbers,
            };

            UserSettings.Current.PasswordGenerator = settings;
            UserSettings.Current.Save();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ClipboardUtilities.SetText(Password, TextDataFormat.UnicodeText, TimeSpan.FromSeconds(UserSettings.Current.ClipboardPersistenceTime));
            var properties = new Dictionary<string, string>();
            properties.Add("PasswordLowercaseLetters", _passwordGenerator.LowercaseLetters ? "true" : "false");
            properties.Add("PasswordLowercaseLettersWithAccentMark", _passwordGenerator.LowercaseLettersWithAccentMark ? "true" : "false");
            properties.Add("PasswordUppercaseLetters", _passwordGenerator.UppercaseLetters ? "true" : "false");
            properties.Add("PasswordUppercaseLettersWithAccentMark", _passwordGenerator.UppercaseLettersWithAccentMark ? "true" : "false");
            properties.Add("PasswordNumbers", _passwordGenerator.Numbers ? "true" : "false");
            properties.Add("PasswordSymbols", _passwordGenerator.Symbols ? "true" : "false");
            var metrics = new Dictionary<string, double>();
            metrics.Add("PasswordLength", _passwordGenerator.Length);
            Telemetry.TrackEvent("Password generated", properties, metrics);
            DialogResult = true;
            Close();
        }

        private void ButtonGenerate_OnClick(object sender, RoutedEventArgs e)
        {
            GeneratePassword();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            GeneratePassword();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            GeneratePassword();
        }
    }
}
