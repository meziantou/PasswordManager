using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Client
{
    public class FieldKey
    {
        [JsonIgnore]
        public Field Field { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        public string FieldId { get; set; }
        public string Key { get; set; }

        [JsonIgnore]
        public FieldKeyData KeyData
        {
            get
            {
                if (Key == null)
                    return null;

                return JsonConvert.DeserializeObject<FieldKeyData>(Key);
            }
            set
            {
                if (value == null)
                {
                    Key = null;
                }
                else
                {
                    Key = JsonConvert.SerializeObject(value);
                }
            }
        }
    }
}