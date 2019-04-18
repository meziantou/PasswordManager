using System;

namespace Meziantou.PasswordManager.Api.Data
{
    public interface ITransaction : IDisposable
    {
        void Commit();
    }
}