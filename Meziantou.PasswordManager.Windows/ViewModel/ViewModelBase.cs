using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Meziantou.PasswordManager.Windows.ViewModel
{
    internal class ViewModelBase : AutoObject
    {
        protected override void Validate(IList<string> errors, string memberName)
        {
            base.Validate(errors, memberName);

            var context = new ValidationContext(this, serviceProvider: null, items: null);
            context.MemberName = memberName;

            var results = new List<ValidationResult>();

            bool isValid;
            if (string.IsNullOrEmpty(memberName))
            {
                isValid = Validator.TryValidateObject(this, context, results);
            }
            else
            {
                // ReSharper disable once ArgumentsStyleNamedExpression
                // ReSharper disable once ExplicitCallerInfoArgument
                var propertyValue = GetProperty<object>(name: memberName);
                isValid = Validator.TryValidateProperty(propertyValue, context, results);
            }

            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errors.Add(validationResult.ErrorMessage);
                }
            }
        }
    }
}