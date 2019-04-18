using System;

namespace Meziantou.PasswordManager.Api.Data
{
    public class DbFieldKey
    {
        public int UserId { get; set; }
        public int FieldId { get; set; }
        public string Key { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }

    public class FieldKey
    {
        public User User { get; set; }
        public string Key { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}