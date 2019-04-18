using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class Document
    {
        public Document()
        {
        }

        public Document(Data.Document document, Data.User currentUser)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            Id = document.Id?.ToString(CultureInfo.InvariantCulture);
            DisplayName = document.DisplayName;
            Tags = document.Tags;
            Fields = new List<Field>(document.Fields.Select(f => new Field(f, currentUser)));

            if (document.IsOwnedBy(currentUser))
            {
                SharedWith = document.Accesses.Select(access => access.User.Username);
            }
            else
            {
                var access = document.FindAccess(currentUser);
                if (access != null)
                {
                    SharedBy = document.User.Username;
                    UserDisplayName = access.DisplayName;
                    UserTags = access.Tags;
                }
            }
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public string UserDisplayName { get; set; }
        public string UserTags { get; set; }
        public IEnumerable<Field> Fields { get; set; }
        public IEnumerable<string> SharedWith { get; set; }
        public string SharedBy { get; set; }
    }
}