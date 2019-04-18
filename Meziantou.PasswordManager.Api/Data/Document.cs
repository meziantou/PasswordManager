using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Meziantou.PasswordManager.Api.Data
{
    public class Document : IId<int?>, ITrackableEntity
    {
        public User User { get; set; }
        public string DisplayName { get; set; }
        public string Tags { get; set; }

        public ICollection<Field> Fields { get; } = new List<Field>();
        public ICollection<DocumentAccess> Accesses { get; } = new List<DocumentAccess>();

        [Key]
        public int? Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public Field FindField(int id)
        {
            return Fields.FirstOrDefault(_ => _.Id == id);
        }
        
        public bool IsOwnedBy(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return User == user;
        }

        public bool IsSharedWith(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return FindAccess(user) != null;
        }

        public DocumentAccess FindAccess(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Accesses.FirstOrDefault(_ => _.User == user);
        }
    }
}