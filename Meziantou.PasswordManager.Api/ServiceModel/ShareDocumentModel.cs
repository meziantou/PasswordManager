using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class ShareDocumentModel
    {
        [Required]
        public string Username { get; set; }

        public IList<FieldKey> Keys { get; set; }
    }
}