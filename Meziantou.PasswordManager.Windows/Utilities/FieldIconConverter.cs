using System;
using System.Globalization;
using System.Windows.Data;
using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.ViewModel;

namespace Meziantou.PasswordManager.Windows.Utilities
{
    public class FieldIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Field field)
            {
                return GetIcon(field.Type);
            }

            if (value is EditableField editableField)
            {
                return GetIcon(editableField.ValueType);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetIcon(FieldValueType type)
        {
            switch (type)
            {
                case FieldValueType.String:
                    return WpfFontAwesome.FontAwesomeIcons.Font;
                case FieldValueType.MultiLineString:
                    return WpfFontAwesome.FontAwesomeIcons.AlignLeft;
                case FieldValueType.Username:
                    return WpfFontAwesome.FontAwesomeIcons.User;
                case FieldValueType.Password:
                    return WpfFontAwesome.FontAwesomeIcons.Lock;
                case FieldValueType.Url:
                    return WpfFontAwesome.FontAwesomeIcons.Link;
                case FieldValueType.EmailAddress:
                    return WpfFontAwesome.FontAwesomeIcons.At;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}