using System;

namespace Meziantou.PasswordManager.Api.Data
{
    public class DbDocumentAccess
    {
        public int UserId { get; set; }
        public int DocumentId { get; set; }

        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }

    public class DocumentAccess
    {
        public User User { get; set; }

        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}