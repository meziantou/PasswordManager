using System.Collections.Generic;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class EncryptedValue
    {
        public byte[] Data { get; set; }
        public IList<EncryptedKey> Keys { get; } = new List<EncryptedKey>();
    }
}
