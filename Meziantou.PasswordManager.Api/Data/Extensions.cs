using System;
using System.Collections.Generic;

namespace Meziantou.PasswordManager.Api.Data
{
    internal static class Extensions
    {
        public static void AddOrReplace<T>(this IList<T> list, T oldItem, T newItem)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            var index = list.IndexOf(oldItem);
            if (index < 0)
            {
                list.Add(newItem);
            }
            else
            {
                list[index] = newItem;
            }
        }

        public static void UpdateTrackingProperties(this ITrackableEntity trackable)
        {
            if (trackable == null)
                return;

            var now = DateTime.UtcNow;
            trackable.LastUpdatedOn = now;

            if (trackable.CreatedOn == default(DateTime))
            {
                trackable.CreatedOn = now;
            }
        }

        public static void SetId<T>(this T item, IEnumerable<T> items) where T : IId<int?>
        {
            if (item.Id.HasValue)
                return;

            var maxValue = 1;
            foreach (var i in items)
            {
                if (i.Id.HasValue && maxValue < i.Id)
                {
                    maxValue = i.Id.Value;
                }
            }

            item.Id = maxValue + 1;
        }
    }
}