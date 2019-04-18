using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    internal class LoginViewModel : ViewModelBase
    {
        public LoginViewModel()
        {
            CanLogIn = true;
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

        public bool RememberMe
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }


        public bool CanLogIn
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string ErrorText
        {
            get => GetProperty(string.Empty);
            set => SetProperty(value);
        }
    }
}