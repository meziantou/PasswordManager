namespace Meziantou.PasswordManager.Client
{
    public interface IMasterKeyProvider
    {
        string GetMasterKey(int attempt);
    }
}