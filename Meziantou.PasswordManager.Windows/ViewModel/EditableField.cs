using System.ComponentModel.DataAnnotations;
using Meziantou.PasswordManager.Client;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    public class EditableField : AutoObject
    {
        [Required]
        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string Selector
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool IsSelectorVisible
        {
            get => GetProperty<bool>(Selector != null);
            set => SetProperty(value);
        }

        public object Value
        {
            get => GetProperty<object>();
            set
            {
                SetProperty(value);
                OnPropertyChanged(nameof(ValueString));
            }
        }

        public FieldValueType ValueType
        {
            get => GetProperty<FieldValueType>();
            set => SetProperty(value);
        }

        public FieldOptions Options
        {
            get => GetProperty<FieldOptions>();
            set => SetProperty(value);
        }

        public string ValueString
        {
            get => Value as string;
            set => Value = value;
        }
    }
}