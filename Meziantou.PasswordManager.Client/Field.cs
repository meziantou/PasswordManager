using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Client
{
    public class Field
    {
        public string Id { get; set; }

        [JsonIgnore]
        public Document Document { get; set; }

        public FieldOptions Options { get; set; }
        public string Name { get; set; }
        public string Selector { get; set; }
        public byte[] Value { get; set; }
        public int SortOrder { get; set; }
        public FieldValueType Type { get; set; }
        public FieldKey Key { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public IList<SharedFieldKey> Keys { get; set; }

        [JsonIgnore]
        public string LastUpdatedOnRelative
        {
            get
            {
                var ts = new TimeSpan(DateTime.UtcNow.Ticks - LastUpdatedOn.Ticks);
                var delta = Math.Abs(ts.TotalSeconds);

                const int second = 1;
                const int minute = 60 * second;
                const int hour = 60 * minute;
                const int day = 24 * hour;
                const int month = 30 * day;

                //if (delta < 1 * minute)
                //    return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

                if (delta < 2 * minute)
                    return "a minute ago";

                if (delta < 45 * minute)
                    return ts.Minutes + " minutes ago";

                if (delta < 90 * minute)
                    return "an hour ago";

                if (delta < 24 * hour)
                    return ts.Hours + " hours ago";

                if (delta < 48 * hour)
                    return "yesterday";

                if (delta < 30 * day)
                    return ts.Days + " days ago";

                if (delta < 12 * month)
                {
                    var months = Convert.ToInt32(Math.Floor((double) ts.Days / 30));
                    return months <= 1 ? "one month ago" : months + " months ago";
                }
                else
                {
                    var years = Convert.ToInt32(Math.Floor((double) ts.Days / 365));
                    return years <= 1 ? "one year ago" : years + " years ago";
                }
            }
        }

        [JsonIgnore]
        public bool IsEncrypted => (Options & FieldOptions.Encrypted) == FieldOptions.Encrypted;

        [JsonIgnore]
        public string DisplayValue
        {
            get
            {
                if (Value == null)
                    return null;

                if (IsEncrypted)
                    return "********";

                return Encoding.UTF8.GetString(Value);
            }
            set
            {
                if (value == null)
                {
                    Value = null;
                }
                else
                {
                    Value = Encoding.UTF8.GetBytes(value);
                }
            }
        }

        public byte[] GetValue()
        {
            return GetValue(Key?.User);
        }

        public byte[] GetValue(User user)
        {
            var value = Value;
            if (value == null)
                return null;

            if (!IsEncrypted)
                return value;

            if (Key == null)
                throw new PasswordManagerException(ErrorCode.NoKeyFound);

            var key = Key.KeyData;
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.IV = key.IV;
                aes.Key = user.DecryptValue(key.Key);

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(value, 0, value.Length);
                    }

                    return ms.ToArray();
                }
            }
        }

        public string GetValueAsString()
        {
            return GetValueAsString(Key?.User);
        }

        public string GetValueAsString(User user)
        {
            var decryptedValue = GetValue(user);
            if (decryptedValue == null)
                return null;

            return Encoding.UTF8.GetString(decryptedValue);
        }
    }
}