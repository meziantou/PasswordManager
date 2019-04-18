using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Api.Data
{
    public class Field : IId<int?>, ITrackableEntity
    {
        public string Name { get; set; }
        public string Selector { get; set; }
        public byte[] Value { get; set; }
        public FieldOptions Options { get; set; }
        public FieldValueType Type { get; set; }
        public int SortOrder { get; set; }
        public IList<FieldKey> Keys { get; } = new List<FieldKey>();

        [JsonIgnore]
        public bool IsEncrypted => (Options & FieldOptions.Encrypted) == FieldOptions.Encrypted;

        [Key]
        public int? Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public FieldKey FindKey(User user)
        {
            return Keys.FirstOrDefault(key => key.User == user);
        }
    }
}