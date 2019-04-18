using Meziantou.PasswordManager.Client;
using Meziantou.PasswordManager.Windows.Settings;
using System;
using System.Threading;
using System.Windows;

namespace Meziantou.PasswordManager.Windows.Utilities
{
    internal class ClipboardUtilities
    {
        private static readonly object Lock = new object();
        private static Timer _timer = null;

        public static void CopyFieldValueToClipboard(Field field)
        {
            if (field != null)
            {
                try
                {
                    var str = field.GetValueAsString();
                    SetText(str, TextDataFormat.UnicodeText, TimeSpan.FromSeconds(UserSettings.Current.ClipboardPersistenceTime));
                }
                catch (PasswordManagerException ex)
                {
                    if (ex.Code == ErrorCode.InvalidMasterKey)
                        return;

                    throw;
                }
            }
        }

        public static void SetText(string value, TextDataFormat format, TimeSpan duration)
        {
            lock (Lock)
            {
                ClearTimer();

                Clipboard.SetText(value, format);

                _timer = new Timer(o =>
                {
                    var thread = new Thread(() =>
                    {
                        if (Clipboard.GetText(format) == value)
                        {
                            Clipboard.Clear();
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                    thread.Start();
                    thread.Join(); //Wait for the thread to end
                    
                    ClearTimer();
                });
                _timer.Change(duration, duration);
            }
        }

        private static void ClearTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
