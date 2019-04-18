using System.Windows;
using System.Windows.Controls;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.ViewModel;

namespace Meziantou.PasswordManager.Windows
{
    public class FieldEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate UrlTemplate { get; set; }
        public DataTemplate UsernameTemplate { get; set; }
        public DataTemplate PasswordTemplate { get; set; }
        public DataTemplate MultiLineStringTemplate { get; set; }
        public DataTemplate EmailAddressTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is EditableField def)
            {
                switch (def.ValueType)
                {
                    case FieldValueType.String:
                        return TextTemplate;

                    case FieldValueType.MultiLineString:
                        return MultiLineStringTemplate;

                    case FieldValueType.Username:
                        return UsernameTemplate;

                    case FieldValueType.Password:
                        return PasswordTemplate;

                    case FieldValueType.Url:
                        return UrlTemplate;

                    case FieldValueType.EmailAddress:
                        return EmailAddressTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}