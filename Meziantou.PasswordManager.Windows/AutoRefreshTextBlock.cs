using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Meziantou.PasswordManager.Windows
{
    public class AutoRefreshTextBlock : TextBlock
    {
        private DispatcherTimer _timer;

        public AutoRefreshTextBlock()
        {
            Loaded += AutoRefreshTextBlock_Loaded;
            Unloaded += AutoRefreshTextBlock_Unloaded;
        }

        private void AutoRefreshTextBlock_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        private void AutoRefreshTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            if (_timer == null)
            {
                var timer = new DispatcherTimer();
                timer.Tick += Timer_Tick;
                timer.Interval = TimeSpan.FromMinutes(1);
                timer.Start();


                _timer = timer;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            GetBindingExpression(TextProperty)?.UpdateTarget();
        }
    }
}