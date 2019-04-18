using System;
using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Api.Data
{
    public class User : IId<int?>, ITrackableEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public int Version { get; set; }

        [Key]
        public int? Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}