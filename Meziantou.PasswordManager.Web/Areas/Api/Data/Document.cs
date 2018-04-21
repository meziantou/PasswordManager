using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class Document : IKeyable<IntId>, ITrackableEntity
    {
        public UserRef User { get; set; }

        [Key]
        public IntId Id { get; set; }
        public string DisplayName { get; internal set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public IList<Field> Fields { get; set; }
        public IList<UserRef> SharedWith { get; set; }

        public bool IsOwnedBy(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return User == user;
        }
    }
}
