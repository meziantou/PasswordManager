using System;

namespace Meziantou.PasswordManager.Api.Data
{
    public class DocumentAccess : ITrackableEntity
    {
        public User User { get; set; }

        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}