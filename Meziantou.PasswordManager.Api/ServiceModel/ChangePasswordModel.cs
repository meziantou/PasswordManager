using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class ChangePasswordModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}