using System;

namespace Meziantou.PasswordManager.Api.Data
{
    public interface ICreatedOnTrackable
    {
        DateTime CreatedOn { get; set; }
    }
}