using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Api.Data
{
    public class DbField
    {
        public string Name { get; set; }
        public byte[] Value { get; set; }
        public FieldOptions Options { get; set; }
        public FieldValueType Type { get; set; }
        public int SortOrder { get; set; }
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }

    public class Field
    {
        public string Name { get; set; }
        public byte[] Value { get; set; }
        public FieldOptions Options { get; set; }
        public FieldValueType Type { get; set; }
        public int SortOrder { get; set; }
        public IList<FieldKey> Keys { get; } = new List<FieldKey>();
        
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}