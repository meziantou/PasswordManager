using System;
using System.Collections.Generic;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
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

        public static void AddOrReplace<T>(this IList<T> list, T item) where T : IKeyable<IntId>
        {
            AddOrReplace<T, IntId>(list, item);
        }

        public static void AddOrReplace<T, TKey>(this IList<T> list, T item) where T : IKeyable<TKey>
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            var id = item.Id;
            for (var i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (Equals(a.Id, id))
                {
                    list[i] = item;
                    return;
                }
            }

            list.Add(item);
        }


        public static void UpdateTrackingProperties(this ITrackableEntity trackable)
        {
            if (trackable == null)
                return;

            var now = DateTime.UtcNow;
            trackable.LastUpdatedOn = now;

            if (trackable.CreatedOn == default)
            {
                trackable.CreatedOn = now;
            }
        }

        public static void SetId<T>(this T item, IEnumerable<T> items) where T : IKeyable<IntId>
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
