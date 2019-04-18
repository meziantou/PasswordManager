namespace Meziantou.PasswordManager.Client
{
    public class UserKey
    {
        public int Version { get; set; }
        public byte[] Salt { get; set; }
        public int IterationCount { get; set; }
        public byte[] IV { get; set; }
        public byte[] Value { get; set; }
    }
}