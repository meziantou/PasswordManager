namespace Meziantou.PasswordManager.Client
{
    public class FieldKeyData
    {
        public int Version { get; set; }
        public byte[] IV { get; set; }
        public byte[] Key { get; set; }
    }
}