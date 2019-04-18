using System;

namespace Meziantou.PasswordManager.Api.Data
{
    public interface ILastUpdatedOnTrackable
    {
        DateTime LastUpdatedOn { get; set; }
    }
}