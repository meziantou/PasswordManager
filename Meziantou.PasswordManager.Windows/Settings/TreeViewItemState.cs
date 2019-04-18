using System.Collections.Generic;
using System.Linq;

namespace Meziantou.PasswordManager.Windows.Settings
{
    public class TreeViewItemState
    {
        public List<string> Path { get; set; }
        public bool IsExpanded { get; set; }

        public bool IsPath(IEnumerable<string> path)
        {
            return Path.SequenceEqual(path);
        }
    }
}