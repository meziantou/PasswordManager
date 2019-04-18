using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Meziantou.PasswordManager.Windows.Settings
{
    public class State
    {
        public List<WindowLocation> WindowLocations { get; set; } = new List<WindowLocation>();
        public List<GridLenthState> GridSplitterPosition { get; set; }
        public string LastSelectedDocumentId { get; set; }
        public TreeViewState DocumentsTreeViewState { get; set; } = new TreeViewState();

        public void SaveWindowLocation(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            var fullTypeName = window.GetType().FullName;
            var location = WindowLocations.FirstOrDefault(l => l.FullTypeName == fullTypeName);
            if (location == null)
            {
                location = new WindowLocation();
                location.FullTypeName = fullTypeName;
                WindowLocations.Add(location);
            }

            location.Top = window.Top;
            location.Left = window.Left;
            location.Width = window.Width;
            location.Height = window.Height;
            location.WindowState = window.WindowState;
        }

        public void RestoreWindowState(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            var fullTypeName = window.GetType().FullName;
            var location = WindowLocations.FirstOrDefault(l => l.FullTypeName == fullTypeName);
            if (location == null)
                return;

            window.Top = location.Top;
            window.Left = location.Left;
            window.Width = location.Width;
            window.Height = location.Height;
            window.WindowState = location.WindowState;
            if (location.WindowState == WindowState.Minimized && UserSettings.Current.MinimizeToSystemTray)
            {
                window.Hide();
            }
        }
    }
}