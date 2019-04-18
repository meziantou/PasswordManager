using Meziantou.PasswordManager.Client;

namespace Meziantou.PasswordManager.Windows.Settings
{
    public class UserSettings
    {
        public static UserSettings Current { get; } = Load();

        public User UserCache { get; set; }
        public PasswordGeneratorSettings PasswordGenerator { get; set; }
        public State State { get; set; } = new State();
        public bool SingleInstance { get; set; } = true;
        public int ClipboardPersistenceTime { get; set; } = 30;
        public int MasterKeyPersistenceTime { get; set; } = 30;
        public bool MasterKeyResetTimerOnAccess { get; set; } = true;
        public bool EnableHttpServer { get; set; } = true;
        public bool RunOnStartUp { get; set; } = true;
        public bool MinimizeToSystemTray { get; set; } = true;

        public void Save()
        {
            SerializationUtilities.SerializeToConfiguration(this);
        }

        private static UserSettings Load()
        {
            return SerializationUtilities.DeserializeFromConfiguration<UserSettings>();
        }

        public void RemoveUserData()
        {
            UserCache = null;
            State.LastSelectedDocumentId = null;
            State.DocumentsTreeViewState.Clear();
            Save();
        }
    }
}