using System;

namespace Meziantou.PasswordManager.Api.Data
{
    public class FieldKey : ITrackableEntity
    {
        public User User { get; set; }
        public string Key { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}