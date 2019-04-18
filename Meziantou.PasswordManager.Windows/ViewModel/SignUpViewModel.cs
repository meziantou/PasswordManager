using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    internal class SignUpViewModel : ViewModelBase
    {
        public SignUpViewModel()
        {
            CanSignUp = true;
        }

        [Required]
        public string Username
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Required]
        public string Password
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Required]
        public string ConfirmPassword
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool CanSignUp
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

            if (string.IsNullOrEmpty(memberName) || memberName == nameof(ConfirmPassword))
            {
                if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
                {
                    errors.Add("Passwords are differents");
                }
            }
        }
    }
}