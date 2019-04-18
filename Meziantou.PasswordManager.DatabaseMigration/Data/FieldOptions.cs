using System;

namespace Meziantou.PasswordManager.Api.Data
{
    [Flags]
    public enum FieldOptions
    {
        None = 0x0,
        Encrypted = 0x1
    }
}