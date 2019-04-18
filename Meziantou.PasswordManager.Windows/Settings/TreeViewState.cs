using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Meziantou.PasswordManager.Windows.Settings
{
    public class TreeViewState
    {
        public List<TreeViewItemState> Nodes { get; set; } = new List<TreeViewItemState>();

        public void Clear()
        {
            Nodes.Clear();
        }

        public void RestoreState(TreeViewItem treeViewItem)
        {
            if (treeViewItem == null)
                return;

            var path = GetFullPath(treeViewItem);
            var state = Nodes.Find(item => item.IsPath(path));
            if (state != null)
            {
                treeViewItem.IsExpanded = state.IsExpanded;
            }
            else
            {
                // Expand only first level
                treeViewItem.IsExpanded = GetParentItem(treeViewItem) == null;
            }
        }

        public void SaveState(TreeViewItem treeViewItem)
        {
            if (treeViewItem == null)
                return;

            var path = GetFullPath(treeViewItem);
            var state = Nodes.Find(item => item.IsPath(path));
            if (state == null)
            {
                state = new TreeViewItemState();
                state.Path = path;
                Nodes.Add(state);
            }

            state.IsExpanded = treeViewItem.IsExpanded;
        }

        private static List<string> GetFullPath(TreeViewItem node)
        {
            if (node == null)
                throw new ArgumentNullException();

            var result = new List<string>();
            result.Add(Convert.ToString(node.Header));

            for (var i = GetParentItem(node); i != null; i = GetParentItem(i))
            {
                result.Insert(0, Convert.ToString(i.Header));
            }

            return result;
        }

        private static TreeViewItem GetParentItem(TreeViewItem item)
        {
            for (var i = LogicalTreeHelper.GetParent(item); i != null; i = LogicalTreeHelper.GetParent(i))
            {
                if (i is TreeViewItem treeViewItem)
                    return treeViewItem;
            }

            return null;
        }
    }
}