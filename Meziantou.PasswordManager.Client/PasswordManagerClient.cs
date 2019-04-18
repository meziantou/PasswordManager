using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Client
{
    public class PasswordManagerClient : IDisposable
    {
        internal const int Version = 1;

        private readonly bool _clientOwned;
        private HttpClient _client;

        public string ApiUrl { get; set; }
        public User User { get; set; }

        public PasswordManagerClient()
        {
#if DEBUG
            ApiUrl = "http://localhost:61157/";
            ApiUrl = "https://api.passwordmanager.meziantou.net/";
#else
            ApiUrl = "https://api.passwordmanager.meziantou.net/";
#endif

            _client = new HttpClient();
            _clientOwned = true;
        }

        public PasswordManagerClient(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _clientOwned = false;
        }

        public void Dispose()
        {
            if (_clientOwned)
            {
                _client?.Dispose();
            }

            _client = null;
        }

        public void SetCredential(string username, string password)
        {
            var headerValue = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", headerValue);

            User = null;
        }

        public void ClearCredential()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            User = null;
        }

        private Uri BuildUri(string str)
        {
            return new Uri(ApiUrl + str, UriKind.RelativeOrAbsolute);
        }

        private async Task EnsureSuccessStatusCode(HttpResponseMessage message)
        {
            if (!message.IsSuccessStatusCode)
            {
                string content = null;
                if (message.Content != null)
                {
                    content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                    message.Content.Dispose();
                }

                if (message.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(content);

                Error error = null;
                try
                {
                    error = JsonConvert.DeserializeObject<Error>(content);
                }
                catch
                {
                    // do nothing
                }

                if (error != null)
                    throw new PasswordManagerException(error.Code, error.Message);

                var s = string.Format(CultureInfo.InvariantCulture, "Status code: {0}; Reason: {1}; Content: {2}", new object[] { message.StatusCode, message.ReasonPhrase, content });
                throw new HttpRequestException(s);
            }
        }

        private async Task<T> GetAsync<T>(string uri) where T : class
        {
            using (var response = await _client.GetAsync(BuildUri(uri)).ConfigureAwait(false))
            {
                await EnsureSuccessStatusCode(response).ConfigureAwait(false);
                var str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (str == null)
                    return null;

                return JsonConvert.DeserializeObject<T>(str);
            }
        }

        private async Task<T> PostAsync<T>(string uri, object data) where T : class
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(BuildUri(uri), content).ConfigureAwait(false))
            {
                await EnsureSuccessStatusCode(response).ConfigureAwait(false);
                var str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (str == null)
                    return null;

                return JsonConvert.DeserializeObject<T>(str);
            }
        }

        private async Task PostAsync(string uri, object data)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(BuildUri(uri), content).ConfigureAwait(false))
            {
                await EnsureSuccessStatusCode(response).ConfigureAwait(false);
            }
        }

        private async Task DeleteAsync(string uri)
        {
            using (var response = await _client.DeleteAsync(BuildUri(uri)).ConfigureAwait(false))
            {
                await EnsureSuccessStatusCode(response).ConfigureAwait(false);
            }
        }

        public async Task<User> MeAsync()
        {
            var user = await GetAsync<User>("user/me").ConfigureAwait(false);
            User = user;
            return user;
        }

        public Task<User> SetVersionAsync(int version)
        {
            return PostAsync<User>("user/me/setversion", new { Version = version });
        }

        public Task<string> GetPublicKeyAsync(string username)
        {
            return GetAsync<string>($"user/{username}/PublicKey");
        }

        public Task<User> SignUpAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", nameof(username));
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", nameof(password));

            return PostAsync<User>("user/signup", new { Username = username, Password = password, Version });
        }

        public Task<User> SetUserKeyAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return PostAsync<User>("user/me/setkey", new { user.PublicKey, user.PrivateKey });
        }

        public Task<User> SetUserKeyAsync(string publicKey, string privateKey)
        {
            return PostAsync<User>("user/me/setkey", new { PublicKey = publicKey, PrivateKey = privateKey });
        }

        public async Task<Document> LoadDocumentAsync(string id)
        {
            var document = await GetAsync<Document>("document/" + id).ConfigureAwait(false);
            await FinishRelation(document).ConfigureAwait(false);
            return document;
        }

        public async Task<IList<Document>> LoadDocumentsAsync()
        {
            var documents = await GetAsync<IList<Document>>("document").ConfigureAwait(false);
            foreach (var document in documents)
            {
                await FinishRelation(document).ConfigureAwait(false);
            }

            return documents;
        }

        public async Task<Document> SaveDocumentAsync(Document document)
        {
            document = await PostAsync<Document>("document", document).ConfigureAwait(false);
            await FinishRelation(document).ConfigureAwait(false);
            return document;
        }

        public Task DeleteDocumentAsync(Document document)
        {
            return DeleteAsync("document/" + document.Id);
        }

        public async Task<IReadOnlyList<UserNameAndPublicKey>> GetUserPublicKeysAsync(Document document)
        {
            var result = new List<UserNameAndPublicKey>();
            if (document.SharedWith != null)
            {
                foreach (var username in document.SharedWith)
                {
                    var key = await GetPublicKeyAsync(username).ConfigureAwait(false);
                    result.Add(new UserNameAndPublicKey(username, key));
                }
            }

            return result;
        }

        public async Task<Document> ShareDocumentAsync(Document document, string username)
        {
            List<FieldKey> fieldKeys = null;
            if (document.HasEncryptedFields)
            {
                var publicKey = await GetPublicKeyAsync(username).ConfigureAwait(false);
                fieldKeys = new List<FieldKey>();
                foreach (var field in document.EncryptedFields)
                {
                    var fkData = field.Key.KeyData;

                    var fieldKeyData = new FieldKeyData();
                    fieldKeyData.Version = 1;
                    fieldKeyData.IV = fkData.IV;

                    var decryptedKey = field.Key.User.DecryptValue(fkData.Key);
                    fieldKeyData.Key = User.EncryptValue(decryptedKey, publicKey);
                    var sharedFk = new FieldKey();
                    sharedFk.FieldId = field.Id;
                    sharedFk.KeyData = fieldKeyData;
                    fieldKeys.Add(sharedFk);
                }
            }

            document = await PostAsync<Document>($"document/{document.Id}/share", new
            {
                Username = username,
                Keys = fieldKeys
            }).ConfigureAwait(false);
            await FinishRelation(document).ConfigureAwait(false);
            return document;
        }

        public async Task UpdateUserAsync(User user, string masterKey)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (user.Version == 0)
            {
                // Update RSA key
                await user.ChangeMasterKeyAsync(masterKey, masterKey);
            }

            user.CopyFrom(await SetVersionAsync(Version));
        }

        private async Task FinishRelation(Document document)
        {
            if (document == null)
                return;

            if (!document.IsSharedBySomeone)
            {
                var user = User;
                if (user == null)
                {
                    user = await MeAsync().ConfigureAwait(false);
                }
                document.User = user;
            }

            foreach (var field in document.Fields)
            {
                field.Document = document;
                await FinishRelation(field).ConfigureAwait(false);
            }
        }

        private async Task FinishRelation(Field field)
        {
            if (field.Key != null)
            {
                field.Key.Field = field;

                var user = User;
                if (user == null)
                {
                    user = await MeAsync().ConfigureAwait(false);
                }

                field.Key.User = user;
            }
        }

        public Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            return PostAsync("user/me/changepassword", new { OldPassword = currentPassword, NewPassword = newPassword });
        }
    }
}