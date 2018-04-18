namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public EncryptedValue EncryptedValue { get; set; }
        public int Type { get; set; }
    }
}
