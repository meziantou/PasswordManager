using System;
using System.Collections.Generic;
using System.Linq;

namespace Meziantou.PasswordManager.Api.Data
{
    public class DbDocument
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }

    public class Document
    {
        public int Id { get; set; }
        public User User { get; set; }
        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public ICollection<Field> Fields { get; } = new List<Field>();
        public ICollection<DocumentAccess> Accesses { get; } = new List<DocumentAccess>();
    }
}