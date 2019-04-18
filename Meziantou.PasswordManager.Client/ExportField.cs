using System;

namespace Meziantou.PasswordManager.Client
{

    public class ExportField
    {
        public string Id { get; set; }
        public FieldOptions Options { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public int SortOrder { get; set; }
        public FieldValueType Type { get; set; }
        public FieldKey Key { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}
