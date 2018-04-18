using System;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public interface ITransaction : IDisposable
    {
        void Commit();
    }
}
