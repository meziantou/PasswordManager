using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Meziantou.Framework.Security;
using System.Net.Http;
using System.Net;
using System.Threading;
using System;

namespace Meziantou.PasswordManager.Client
{
    public class PasswordManagerContext : INotifyPropertyChanged
    {
#if DEBUG
        private const string CredentialManagerName = "Meziantou.PasswordManager - DEBUG";
#else
        private const string CredentialManagerName = "Meziantou.PasswordManager";
#endif

        private static PasswordManagerContext _current;
        private User _user;
        private bool _clientOwned = false;
        private PasswordManagerClient _client;

        public static PasswordManagerContext Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new PasswordManagerContext();
                }

                return _current;
            }
        }

        public IMasterKeyProvider MasterKeyProvider { get; set; }
        public TemporaryKeyStore MasterKeyStore { get; set; }

        public User User
        {
            get => _user;
            set
            {
                if (Equals(value, _user)) return;
                _user = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn => User != null;

        public bool CanAutoLogin => CredentialManager.ReadCredential(CredentialManagerName) != null;

        public bool CanCacheData => CanAutoLogin;

        public Task AutoLoginAsync()
        {
            var cred = CredentialManager.ReadCredential(CredentialManagerName);
            if (cred != null)
            {
                return LoginAsync(cred.UserName, cred.Password, true);
            }

            return Task.CompletedTask;
        }

        public async Task LoginAsync(string username, string password, bool rememberMe)
        {
            Client.SetCredential(username, password);

            try
            {
                User = await Client.MeAsync();
            }
            catch (HttpRequestException hre)
            {
                if (hre.InnerException is WebException we)
                {
                    if (we.Status == WebExceptionStatus.ConnectFailure)
                        throw; // no internet connection
                }

                LogOut(); // Invalid creds
                throw;
            }

            if (rememberMe)
            {
                try
                {
                    CredentialManager.WriteCredential(CredentialManagerName, username, password, CredentialPersistence.LocalMachine);
                }
                catch
                {
                    // Do not block the login process...
                    // TODO log
                }
            }
        }

        public void LogOut()
        {
            ClearMasterKey();
            Client.ClearCredential();
            ClearPersistedCredential();

            User = null;
        }

        private static void ClearPersistedCredential()
        {
            if (CredentialManager.ReadCredential(CredentialManagerName) != null)
            {
                try
                {
                    CredentialManager.DeleteCredential(CredentialManagerName);
                }
                catch
                {
                    // TODO log
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetMasterKey(int attempt)
        {
            if (MasterKeyStore.Key == null)
            {
                var masterKey = MasterKeyProvider.GetMasterKey(attempt);
                if (string.IsNullOrEmpty(masterKey))
                    return null;

                MasterKeyStore.Key = masterKey;
            }

            return MasterKeyStore.Key;
        }

        public void ClearMasterKey()
        {
            MasterKeyStore.Clear();
        }
        
        public PasswordManagerClient Client
        {
            get
            {
                var client = _client;
                if (client == null)
                {
                    client = new PasswordManagerClient();
                    _client = client;
                    _clientOwned = true;
                }

                return client;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                var oldClient = _client;
                if(oldClient != null && _clientOwned)
                {
                    oldClient.Dispose();
                }

                _client = value;
            }
        }
    }
}