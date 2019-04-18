namespace Meziantou.PasswordManager.Windows
{
    internal enum CreateFieldType
    {
        [DefaultName("Text")]
        String = 0,
        [DefaultName("Notes")]
        MultiLineString = 1,
        [DefaultName("User name")]
        Username = 2,
        [DefaultName("Password")]
        Password = 3,
        [DefaultName("Url")]
        Url = 4,
        [DefaultName("Email address")]
        EmailAddress = 5,
        [DefaultName("Notes")]
        EncryptedMultiLineString = 6
    }
}