using System;
using System.Globalization;
using System.Linq;
using Meziantou.PasswordManager.Api.Data;
using System.Collections.Generic;

namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class Field
    {
        public Field()
        {
        }

        public Field(Data.Field field, Data.User currentUser)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            Id = field.Id?.ToString(CultureInfo.InvariantCulture);
            Name = field.Name;
            Selector = field.Selector;
            Value = field.Value;
            Options = field.Options;
            Type = field.Type;
            SortOrder = field.SortOrder;
            LastUpdatedOn = field.LastUpdatedOn;

            if (field.Keys != null)
            {
                var key = field.FindKey(currentUser);
                if (key != null)
                {
                    Key = new FieldKey() { FieldId = field.Id?.ToString(CultureInfo.InvariantCulture), Key = key.Key };
                }
            }
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Selector { get; set; }
        public byte[] Value { get; set; }
        public FieldOptions Options { get; set; }
        public FieldValueType Type { get; set; }
        public int SortOrder { get; set; }
        public FieldKey Key { get; set; }
        public IEnumerable<SharedFieldKey> Keys { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}