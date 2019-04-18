namespace Meziantou.PasswordManager.Windows.Settings
{
    public class PasswordGeneratorSettings
    {
        public bool LowercaseLetters { get; set; }
        public bool LowercaseLettersWithAccentMark { get; set; }
        public bool UppercaseLetters { get; set; }
        public bool UppercaseLettersWithAccentMark { get; set; }
        public bool Numbers { get; set; }
        public bool Symbols { get; set; }
        public int Length { get; set; }
    }
}