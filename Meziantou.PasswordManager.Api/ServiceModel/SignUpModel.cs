using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class SignUpModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int Version { get; set; }
    }
}