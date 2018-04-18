namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public interface IKeyable<T>
    {
        T Id { get; set; }
    }
}
