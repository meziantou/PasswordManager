using System.Collections.Generic;

namespace Meziantou.PasswordManager.Client
{
    public class ExportDocument
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public string SharedBy { get; set; }
        public IList<string> SharedWith { get; set; }
        public IList<ExportField> Fields { get; set; }
    }
}
