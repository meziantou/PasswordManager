using Meziantou.Framework.Utilities;
using System;
using System.Collections.Generic;

namespace Meziantou.PasswordManager.Windows.Utilities
{
    public static class Extensions
    {
        public static void AddRange<T>(this ICollection<T> collection, params T[] items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static bool ContainsIgnoreCase(this string str, string value)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool ContainsIgnoreCase(this IEnumerable<string> str, string value)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            foreach (var s in str)
            {
                if (s.EqualsIgnoreCase(value))
                    return true;
            }

            return false;
        }
    }
}