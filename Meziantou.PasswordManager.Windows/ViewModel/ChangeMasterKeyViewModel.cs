using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    internal class ChangeMasterKeyViewModel : ViewModelBase
    {
        public ChangeMasterKeyViewModel()
        {
            CanGenerate = true;
        }

        [Required]
        public string CurrentMasterKey
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Required]
        public string MasterKey
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [Required]
        public string ConfirmMasterKey
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

            if (string.IsNullOrEmpty(memberName) || memberName == nameof(ConfirmMasterKey))
            {
                if (!string.IsNullOrEmpty(MasterKey) && MasterKey != ConfirmMasterKey)
                {
                    errors.Add("Keys are differents");
                }
            }
        }
    }
}