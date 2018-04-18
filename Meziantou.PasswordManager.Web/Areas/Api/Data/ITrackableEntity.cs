using System;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public interface ITrackableEntity
    {
        DateTime CreatedOn { get; set; }
        DateTime LastUpdatedOn { get; set; }
    }
}
