using System.Collections.Generic;

namespace Meziantou.PasswordManager.Web.Areas.Api.ServiceModel
{
    public class Document
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Tags { get; set; }
        public string UserDisplayName { get; set; }
        public string UserTags { get; set; }
        public IEnumerable<Field> Fields { get; set; }
        public IEnumerable<string> SharedWith { get; set; }
        public string SharedBy { get; set; }
    }
}
