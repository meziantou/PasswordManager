using System;
using System.Threading;

namespace Meziantou.PasswordManager.Client
{
    public class TemporaryKeyStore : IDisposable
    {
        private string _key;
        private Timer _timer;

        public bool ResetTimerOnAccess { get; set; }
        public TimeSpan KeyDuration { get; set; } = TimeSpan.FromMinutes(1);

        public string Key
        {
            get
            {
                if (ResetTimerOnAccess)
                {
                    _timer?.Change(KeyDuration, TimeSpan.Zero);
                }

                return _key;
            }
            set
            {
                Clear();
                _key = value;
                _timer = new Timer(o => Clear());
                _timer.Change(KeyDuration, TimeSpan.Zero);
            }
        }

        public void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            _key = null;
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}