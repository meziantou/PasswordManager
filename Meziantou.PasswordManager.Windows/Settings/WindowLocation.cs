using System.Windows;

namespace Meziantou.PasswordManager.Windows.Settings
{
    public class WindowLocation
    {
        public string FullTypeName { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public WindowState WindowState { get; set; }
    }
}