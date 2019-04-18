using System;

namespace Meziantou.PasswordManager.Windows
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class DefaultNameAttribute : Attribute
    {
        public DefaultNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static string GetName(CreateFieldType createFieldType)
        {
            var memInfo = typeof(CreateFieldType).GetMember(createFieldType.ToString());
            if (memInfo == null || memInfo.Length == 0)
                return null;

            var attributes = memInfo[0].GetCustomAttributes(typeof(DefaultNameAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return ((DefaultNameAttribute)attributes[0]).Name;

            return null;
        }
    }
}