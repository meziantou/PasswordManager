namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class EncryptedKey
    {
        public User User { get; set; }
        public byte[] Key { get; set; }
    }
}
