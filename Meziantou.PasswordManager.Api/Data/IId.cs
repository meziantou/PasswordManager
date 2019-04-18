namespace Meziantou.PasswordManager.Api.Data
{
    public interface IId<T>
    {
        T Id { get; set; }
    }
}