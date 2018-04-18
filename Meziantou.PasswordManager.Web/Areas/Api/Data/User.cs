using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class User : IKeyable<IntId>, ITrackableEntity
    {
        [Key]
        public IntId Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        public JObject PublicKey { get; set; }
        public JObject PrivateKey { get; set; }
        public int Version { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}
