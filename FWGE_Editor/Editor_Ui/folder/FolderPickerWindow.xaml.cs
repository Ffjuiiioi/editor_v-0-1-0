using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FWGE_Editor.Editor_Ui.folder
{
    public partial class FolderPickerWindow : Window
    {
        public string? SelectedPath { get; private set; }

        public FolderPickerWindow()
        {
            InitializeComponent();
            LoadDrives();
        }

        private void LoadDrives()
        {
            FolderTree.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var item = CreateTreeItem(drive.Name, drive.RootDirectory.FullName, isFolder: true);
                    item.Items.Add(new TreeViewItem()); // placeholder
                    item.Expanded += Folder_Expanded;
                    FolderTree.Items.Add(item);
                }
            }
        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;

            if (item.Items.Count == 1 && item.Items[0] is TreeViewItem placeholder && placeholder.Header == null)
            {
                item.Items.Clear();
                string path = item.Tag as string ?? "";

                try
                {
                    // Add directories
                    foreach (string dir in Directory.GetDirectories(path))
                    {
                        if ((File.GetAttributes(dir) & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                        {
                            var subItem = CreateTreeItem(System.IO.Path.GetFileName(dir), dir, isFolder: true);
                            subItem.Items.Add(new TreeViewItem()); // placeholder
                            subItem.Expanded += Folder_Expanded;
                            item.Items.Add(subItem);
                        }
                    }

                    // Add files
                    foreach (string file in Directory.GetFiles(path))
                    {
                        if ((File.GetAttributes(file) & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                        {
                            var fileItem = CreateTreeItem(System.IO.Path.GetFileName(file), file, isFolder: false);
                            item.Items.Add(fileItem);
                        }
                    }
                }
                catch { /* Ignore access exceptions */ }
            }
        }

        private TreeViewItem CreateTreeItem(string name, string path, bool isFolder)
        {
            string text = $"{(isFolder ? "📁" : "📄")} {name}";
            return new TreeViewItem
            {
                Header = text,
                Tag = path
            };
        }

        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (FolderTree.SelectedItem is TreeViewItem selected && selected.Tag is string tag)
            {
                SelectedPath = tag;

                // Auto-expand if it's a folder with placeholder
                if (Directory.Exists(tag) && selected.Items.Count == 1 && selected.Items[0] is TreeViewItem placeholder && placeholder.Header == null)
                {
                    Folder_Expanded(selected, null!);
                }
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedPath))
            {
                DialogResult = true;
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a folder or file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
