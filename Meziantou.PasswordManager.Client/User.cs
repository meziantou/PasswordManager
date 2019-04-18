using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace Meziantou.PasswordManager.Client
{
    public class User : INotifyPropertyChanged
    {
        private IList<Document> _documents;
        private string _username;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public IList<Document> Documents
        {
            get => _documents;
            set
            {
                _documents = value;
                OnPropertyChanged();
            }
        }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public int Version { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Document CreateNewDocument()
        {
            var document = new Document();
            document.User = this;
            return document;
        }

        public void GenerateKey(string masterKey, int keySize = 4096)
        {
            using (var csp = new RSACryptoServiceProvider(keySize))
            {
                var privateKey = RSAKeyExtensions.ToJsonString(csp, true);
                var publicKey = RSAKeyExtensions.ToJsonString(csp, false);

                privateKey = EncryptPrivateKey(masterKey, privateKey);

                PublicKey = publicKey;
                PrivateKey = privateKey;
            }
        }

        private static string EncryptPrivateKey(string masterKey, string privateKey)
        {
            using (var derivedKey = new Rfc2898DeriveBytes(masterKey, 32, 50000))
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Key = derivedKey.GetBytes(256 / 8);

                    byte[] encryptedPrivateKey;
                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] value = Encoding.ASCII.GetBytes(privateKey);
                            cryptoStream.Write(value, 0, value.Length);
                        }

                        encryptedPrivateKey = ms.ToArray();
                    }


                    var userKey = new UserKey();
                    userKey.Version = 1;
                    userKey.Salt = derivedKey.Salt;
                    userKey.IterationCount = derivedKey.IterationCount;
                    userKey.IV = aes.IV;
                    userKey.Value = encryptedPrivateKey;
                    privateKey = JsonConvert.SerializeObject(userKey);
                }
            }
            return privateKey;
        }

        private string DecryptPrivateKey(string masterKey)
        {
            if (PrivateKey == null)
                return null;

            var userKey = JsonConvert.DeserializeObject<UserKey>(PrivateKey);

            using (var derivedKey = new Rfc2898DeriveBytes(masterKey, userKey.Salt, userKey.IterationCount))
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = derivedKey.GetBytes(256 / 8);
                aes.IV = userKey.IV;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        byte[] value = userKey.Value;
                        cryptoStream.Write(value, 0, value.Length);
                    }

                    return Encoding.ASCII.GetString(ms.ToArray());
                }
            }
        }

        private static RSA GetRsa(string configuration)
        {
            // V1
            if (configuration.StartsWith("{"))
            {
                var rsa = RSA.Create();
                RSAKeyExtensions.FromJsonString(rsa, configuration);
                return rsa;
            }

            // V0
            if (configuration.StartsWith("<"))
            {
                var rsa = RSA.Create();
                RSAKeyExtensions.FromXmlString(rsa, configuration);
                return rsa;
            }

            throw new ArgumentException("Private key format not supported", nameof(configuration));
        }

        public async Task ChangeMasterKeyAsync(string oldMasterKey, string newMasterKey)
        {
            string privateKey;
            try
            {
                privateKey = DecryptPrivateKey(oldMasterKey);
                using (var rsa = GetRsa(privateKey))
                {
                    // Change both private and public keys, so we are using the lastest format
                    var privateKeyJson = RSAKeyExtensions.ToJsonString(rsa, true);
                    var publicKeyJson = RSAKeyExtensions.ToJsonString(rsa, false);

                    privateKey = EncryptPrivateKey(newMasterKey, privateKeyJson);
                    
                    var context = PasswordManagerContext.Current;
                    var user = await context.Client.SetUserKeyAsync(publicKeyJson, privateKey);
                    context.ClearMasterKey();
                    CopyFrom(user);
                }
            }
            catch (CryptographicException ex)
            {
                throw new PasswordManagerException(ErrorCode.InvalidMasterKey, "Invalid masterkey", ex);
            }
        }

        public string ExportDocumentToJson(out IList<Document> errors)
        {
            errors = null;
            var result = new List<ExportDocument>();
            foreach (var document in Documents)
            {
                try
                {
                    var exportDocument = new ExportDocument();
                    exportDocument.Id = document.Id;
                    exportDocument.DisplayName = document.FinalDisplayName;
                    exportDocument.SharedBy = document.SharedBy;
                    exportDocument.SharedWith = document.SharedWith;
                    exportDocument.Tags = document.FinalTags;

                    if (document.Fields != null)
                    {
                        exportDocument.Fields = new List<ExportField>();
                        foreach (var field in document.Fields)
                        {
                            var exportField = new ExportField();
                            exportField.Id = field.Id;
                            exportField.Name = field.Name;
                            exportField.Options = field.Options;
                            exportField.Type = field.Type;
                            exportField.SortOrder = field.SortOrder;
                            exportField.LastUpdatedOn = field.LastUpdatedOn;
                            exportField.Value = field.GetValueAsString();

                            exportDocument.Fields.Add(exportField);
                        }
                    }

                    result.Add(exportDocument);
                }
                catch
                {
                    if (errors == null)
                    {
                        errors = new List<Document>();
                    }

                    errors.Add(document);
                }
            }

            return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        internal void CopyFrom(User user)
        {
            if (user == null)
                return;

            Username = user.Username;
            PrivateKey = user.PrivateKey;
            PublicKey = user.PublicKey;
            Version = user.Version;
        }

        public byte[] EncryptValue(byte[] value)
        {
            return EncryptValue(value, PublicKey);
        }

        public static byte[] EncryptValue(byte[] value, string publicKey)
        {
            using (var rsa = GetRsa(publicKey))
            {
                return rsa.Encrypt(value, RSAEncryptionPadding.OaepSHA1);
            }
        }

        public byte[] DecryptValue(byte[] value, DecryptValueOptions options = null)
        {
            if (options == null)
            {
                options = new DecryptValueOptions();
            }

            var attempt = 1;
            while (true)
            {
                var context = PasswordManagerContext.Current;
                var masterKey = context.GetMasterKey(attempt);
                if (string.IsNullOrEmpty(masterKey))
                {
                    context.ClearMasterKey();
                    throw new PasswordManagerException(ErrorCode.InvalidMasterKey, "Invalid masterkey");
                }

                try
                {
                    var privateKey = DecryptPrivateKey(masterKey);
                    using (var rsa = GetRsa(privateKey))
                    {
                        return rsa.Decrypt(value, RSAEncryptionPadding.OaepSHA1);
                    }
                }
                catch (CryptographicException ex)
                {
                    context.ClearMasterKey();

                    attempt++;
                    if (attempt <= options.MaxRetryCount)
                        continue;

                    throw new PasswordManagerException(ErrorCode.InvalidMasterKey, "Invalid masterkey", ex);
                }
            }
        }

        public bool IsUpToDate()
        {
            return Version >= PasswordManagerClient.Version;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}