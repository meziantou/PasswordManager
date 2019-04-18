using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Meziantou.PasswordManager.Windows.Utilities
{
    internal static class NavigationUtilities
    {
        public static readonly DependencyProperty IsNavigationContainerProperty = DependencyProperty.RegisterAttached(
            "IsNavigationContainer", typeof(bool), typeof(NavigationUtilities), new PropertyMetadata(default(bool)));

        public static void SetIsNavigationContainer(DependencyObject element, bool value)
        {
            element.SetValue(IsNavigationContainerProperty, value);
        }

        public static bool GetIsNavigationContainer(DependencyObject element)
        {
            return (bool) element.GetValue(IsNavigationContainerProperty);
        }

        public static void Navigate(this DependencyObject element, object content)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            while (element != null)
            {
                if (element is MainWindow mainWindow)
                {
                    mainWindow.Navigate(content);
                    return;
                }

                if (GetIsNavigationContainer(element))
                {
                    if (element is ContentControl control)
                    {
                        control.Content = content;
                        return;
                    }
                }

                element = VisualTreeHelper.GetParent(element);
            }
        }

        public static void NavigateToAutoLoginPage(this DependencyObject element)
        {
            Navigate(element, new LoginAutoPage());
        }

        public static void NavigateToMainPage(this DependencyObject element)
        {
            Navigate(element, new MainPage());
        }

        public static void NavigateToLoginPage(this DependencyObject element)
        {
            Navigate(element, new LoginPage());
        }

        public static void NavigateToLoadingUserDataPage(this DependencyObject element)
        {
            Navigate(element, new LoadingUserDataPage());
        }

        public static void NavigateToCreateUserKeyPage(this DependencyObject element)
        {
            Navigate(element, new CreateMasterKeyPage());
        }

        public static void NavigateToSignUpPage(this DependencyObject element)
        {
            Navigate(element, new SignUpPage());
        }

        public static void NavigateToUpdateUserPage(this DependencyObject element)
        {
            Navigate(element, new UpdateUserPage());
        }
    }
}