using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    internal class ChangePasswordViewModel : ViewModelBase
    {
        public ChangePasswordViewModel()
        {
            CanGenerate = true;
        }

        [Required]
        public string CurrentPassword
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Required]
        public string NewPassword
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Required]
        public string ConfirmNewPassword
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool CanGenerate
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string ErrorText
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        protected override void Validate(IList<string> errors, string memberName)
        {
            base.Validate(errors, memberName);

            if (string.IsNullOrEmpty(memberName) || memberName == nameof(ConfirmNewPassword))
            {
                if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmNewPassword)
                {
                    errors.Add("Keys are differents");
                }
            }
        }
    }
}