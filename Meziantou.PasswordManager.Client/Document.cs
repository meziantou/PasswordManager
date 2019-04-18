using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Client
{
    public class Document
    {
        [JsonIgnore]
        public readonly char[] TagSeparator = { ',' };
        private ICollection<string> _tagList;
        private string _tags;

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UserDisplayName { get; set; }
        public string UserTags { get; set; }

        public string Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                _tagList = null;
            }
        }

        public string FinalTags => UserTags ?? Tags;

        [JsonIgnore]
        public IEnumerable<string> FinalTagList
        {
            get
            {
                if (_tagList == null)
                {
                    if (FinalTags == null)
                    {
                        _tagList = new string[0];
                    }
                    else
                    {
                        _tagList = FinalTags.Split(TagSeparator, StringSplitOptions.None)
                            .Select(tag => tag.Trim())
                            .Distinct()
                            .ToList();
                    }
                }

                return _tagList;
            }
        }

        [JsonIgnore]
        public string FinalDisplayName
        {
            get
            {
                string name = null;
                if (!string.IsNullOrEmpty(UserDisplayName))
                {
                    name = UserDisplayName;
                }
                else if (!string.IsNullOrEmpty(DisplayName))
                {
                    name = DisplayName;
                }
                else
                {
                    if (Fields.Any())
                    {
                        var displayField = Fields.FirstOrDefault(f => !f.IsEncrypted);
                        if (displayField != null)
                        {
                            name = displayField.DisplayValue;
                        }
                    }

                    if (name != null)
                    {
                        if (Uri.TryCreate(name, UriKind.Absolute, out Uri uri))
                        {
                            name = uri.Host;
                        }
                    }
                }

                return name;
            }
        }

        [JsonIgnore]
        public User User { get; set; }

        public string SharedBy { get; set; }
        public IList<string> SharedWith { get; set; } = new ObservableCollection<string>();
        public IList<Field> Fields { get; } = new ObservableCollection<Field>();

        [JsonIgnore]
        public bool IsSharedBySomeone => SharedBy != null;

        [JsonIgnore]
        public bool IsSharedWithSomeone => SharedWith != null && SharedWith.Any();

        [JsonIgnore]
        public string SharedWithString
        {
            get
            {
                if (SharedWith == null)
                    return null;

                return string.Join(", ", SharedWith.OrderBy(s => s));
            }
        }

        [JsonIgnore]
        public IEnumerable<Field> EncryptedFields => Fields.Where(field => field.IsEncrypted);

        [JsonIgnore]
        public bool HasEncryptedFields => Fields.Any(field => field.IsEncrypted);

        private Field CreateField(string name, FieldValueType valueType, string selector)
        {
            var field = new Field
            {
                Document = this,
                Name = name,
                Type = valueType,
                Selector = selector,
                SortOrder = Fields.Count
            };
            return field;
        }

        public Field AddField(string name, string value, FieldValueType valueType, string selector)
        {
            var field = CreateField(name, valueType, selector);
            field.DisplayValue = value;
            Fields.Add(field);
            return field;
        }

        public Field AddEncryptedField(string name, string value, FieldValueType valueType, string selector, IEnumerable<UserNameAndPublicKey> additionalUsers)
        {
            return AddEncryptedField(name, Encoding.UTF8.GetBytes(value), valueType, selector, additionalUsers);
        }

        public Field AddEncryptedField(string name, byte[] value, FieldValueType valueType, string selector, IEnumerable<UserNameAndPublicKey> additionalUsers)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;

                var field = CreateField(name, valueType, selector);
                field.Options |= FieldOptions.Encrypted;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(value, 0, value.Length);
                    }

                    field.Value = ms.ToArray();
                }

                var fieldKeyData = new FieldKeyData
                {
                    Version = 1,
                    IV = aes.IV,
                    Key = User.EncryptValue(aes.Key)
                };

                var fieldKey = new FieldKey
                {
                    Field = field,
                    KeyData = fieldKeyData
                };

                field.Key = fieldKey;
                Fields.Add(field);

                if (additionalUsers != null)
                {
                    foreach (var user in additionalUsers)
                    {
                        var username = user.Username;
                        var publicKey = user.PublicKey;

                        var sharedFieldKeyData = new FieldKeyData
                        {
                            Version = 1,
                            IV = aes.IV,
                            Key = User.EncryptValue(aes.Key, publicKey)
                        };

                        var sharedFieldKey = new SharedFieldKey
                        {
                            Username = username,
                            KeyData = sharedFieldKeyData
                        };

                        if (field.Keys == null)
                        {
                            field.Keys = new List<SharedFieldKey>();
                        }

                        field.Keys.Add(sharedFieldKey);
                    }
                }

                return field;
            }
        }

        public override string ToString()
        {
            return FinalDisplayName;
        }

        public bool MatchSearchUrl(string searchUrl)
        {
            var urlFields = Fields.Where(field => !field.IsEncrypted && field.Type == FieldValueType.Url);
            foreach (var field in urlFields)
            {
                var fieldValue = field.GetValueAsString();
                if (SearchUrl(searchUrl, fieldValue))
                    return true;
            }

            return false;
        }

        private bool SearchUrl(string searchUrl, string fieldUrl)
        {
            searchUrl = NormalizeUrlForSearching(searchUrl);
            fieldUrl = NormalizeUrlForSearching(fieldUrl);

            if (searchUrl.StartsWith(fieldUrl, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!Uri.TryCreate(searchUrl, UriKind.RelativeOrAbsolute, out var searchUri))
                return false;

            if (!Uri.TryCreate(fieldUrl, UriKind.RelativeOrAbsolute, out var fieldUri))
                return false;

            if (!AreEquivalentScheme(searchUri, fieldUri))
                return false;

            if (!string.Equals(GetHost(searchUri), GetHost(fieldUri), StringComparison.OrdinalIgnoreCase))
                return false;

            if ((IsDefaultPort(searchUri) == false || IsDefaultPort(fieldUri) == false) && GetPort(searchUri) != GetPort(fieldUri))
                return false;

            return true;
        }

        private bool? IsDefaultPort(Uri a)
        {
            try
            {
                return a.IsDefaultPort;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private bool AreEquivalentScheme(Uri a, Uri b)
        {
            const string defaultScheme = "http";
            var schemeA = GetScheme(a) ?? defaultScheme;
            var schemeB = GetScheme(b) ?? defaultScheme;
            if (string.Equals(schemeA, schemeB, StringComparison.OrdinalIgnoreCase))
                return true;

            if (IsHttpScheme(schemeA) && IsHttpScheme(schemeB))
                return true;

            return false;
        }

        private static bool IsHttpScheme(string scheme)
        {
            return scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps;
        }

        private static string GetScheme(Uri uri)
        {
            try
            {
                return uri.Scheme;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static string GetHost(Uri uri)
        {
            try
            {
                return uri.Host;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static int GetPort(Uri uri)
        {
            try
            {
                return uri.Port;
            }
            catch (InvalidOperationException)
            {
                return -1;
            }
        }

        private string NormalizeUrlForSearching(string url)
        {
            if (url == null)
                return url;

            while (url.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }
    }
}