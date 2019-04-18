using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class SetVersionModel
    {
        [Required]
        public int Version { get; set; }
    }
}