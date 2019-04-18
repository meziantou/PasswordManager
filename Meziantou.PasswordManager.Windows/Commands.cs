using System.Windows.Input;

namespace Meziantou.PasswordManager.Windows
{
    internal class Commands
    {
        private static RoutedUICommand _refreshCommand;
        private static RoutedUICommand _logOutCommand;
        private static RoutedUICommand _closeCommand;
        private static RoutedUICommand _changeMasterKeyCommand;
        private static RoutedUICommand _changePasswordCommand;
        private static RoutedUICommand _exportDocumentsCommand;
        private static RoutedUICommand _testPasswordsOfflineCommand;
        private static RoutedUICommand _testPasswordsOnlineCommand;
        private static RoutedUICommand _openMainWindowCommand;

        public static RoutedUICommand LogOut
        {
            get
            {
                if (_logOutCommand == null)
                {
                    _logOutCommand = new RoutedUICommand("Log Out", "LogOut", typeof(Commands));
                }

                return _logOutCommand;
            }
        }

        public static RoutedUICommand Close
        {
            get
            {
                if (_closeCommand == null)
                {
                    var gestures = new InputGestureCollection();
                    gestures.Add(new KeyGesture(Key.F4, ModifierKeys.Alt));
                    _closeCommand = new RoutedUICommand("Exit", "Exit", typeof(Commands), gestures);
                }

                return _closeCommand;
            }
        }

        public static RoutedUICommand Refresh
        {
            get
            {
                if (_refreshCommand == null)
                {
                    var gestures = new InputGestureCollection();
                    gestures.Add(new KeyGesture(Key.F5));
                    _refreshCommand = new RoutedUICommand("Refresh", "Refresh", typeof(Commands), gestures);
                }

                return _refreshCommand;
            }
        }

        public static RoutedUICommand ChangeMasterKey
        {
            get
            {
                if (_changeMasterKeyCommand == null)
                {
                    _changeMasterKeyCommand = new RoutedUICommand("Change Master Key", "ChangeMasterKey", typeof(Commands));
                }

                return _changeMasterKeyCommand;
            }
        }

        public static RoutedUICommand ChangePassword
        {
            get
            {
                if (_changePasswordCommand == null)
                {
                    _changePasswordCommand = new RoutedUICommand("Change Password", "ChangePassword", typeof(Commands));
                }

                return _changePasswordCommand;
            }
        }

        public static RoutedUICommand ExportDocuments
        {
            get
            {
                if (_exportDocumentsCommand == null)
                {
                    _exportDocumentsCommand = new RoutedUICommand("Export Documents", "ExportDocuments", typeof(Commands));
                }

                return _exportDocumentsCommand;
            }
        }

        public static RoutedUICommand TestPasswordsOffline
        {
            get
            {
                if (_testPasswordsOfflineCommand == null)
                {
                    _testPasswordsOfflineCommand = new RoutedUICommand("I have been pwned? (Offline)", "TestPasswordsOffline", typeof(Commands));
                }

                return _testPasswordsOfflineCommand;
            }
        }

        public static RoutedUICommand TestPasswordsOnline
        {
            get
            {
                if (_testPasswordsOnlineCommand == null)
                {
                    _testPasswordsOnlineCommand = new RoutedUICommand("I have been pwned? (online)", "TestPasswordsOnline", typeof(Commands));
                }

                return _testPasswordsOnlineCommand;
            }
        }

        public static RoutedUICommand OpenMainWindowCommand
        {
            get
            {
                if (_openMainWindowCommand == null)
                {
                    _openMainWindowCommand = new RoutedUICommand("Open Main Window", "OpenMainWindow", typeof(Commands));
                }

                return _openMainWindowCommand;
            }
        }
    }
}