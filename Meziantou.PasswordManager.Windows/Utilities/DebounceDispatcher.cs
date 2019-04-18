using System;
using System.Windows.Threading;

namespace Meziantou.PasswordManager.Windows.Utilities
{
    public class DebounceDispatcher
    {
        private DispatcherTimer _timer;
        private DateTime TimerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);

        public void Debounce(int interval, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            // kill pending timer and pending ticks
            _timer?.Stop();
            _timer = null;

            if (disp == null)
                disp = Dispatcher.CurrentDispatcher;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (_timer == null)
                    return;

                _timer?.Stop();
                _timer = null;
                action.Invoke(param);
            }, disp);

            _timer.Start();
        }

        public void Throttle(int interval, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            // kill pending timer and pending ticks
            _timer?.Stop();
            _timer = null;

            if (disp == null)
                disp = Dispatcher.CurrentDispatcher;

            var curTime = DateTime.UtcNow;

            // if timeout is not up yet - adjust timeout to fire 
            // with potentially new Action parameters           
            if (curTime.Subtract(TimerStarted).TotalMilliseconds < interval)
                interval -= (int)curTime.Subtract(TimerStarted).TotalMilliseconds;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (_timer == null)
                    return;

                _timer?.Stop();
                _timer = null;
                action.Invoke(param);
            }, disp);

            _timer.Start();
            TimerStarted = curTime;
        }
    }
}
