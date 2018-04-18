using System.Collections.Generic;

namespace Meziantou.PasswordManager.Web.Areas.Api.ServiceModel
{
    public class EncryptedValue
    {
        public string Data { get; set; }
        public string Key { get; set; }
        public IList<AdditionalKey> AdditionalKeys { get; set; }
    }
}
