using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace FileDuplicatesFinder
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        static string path = "";
        private void SpecifyPath_button_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();

            var result = fbd.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            path = fbd.SelectedPath;

            Path_label.Content = fbd.SelectedPath;
        }


        
        private void Go_button_Click(object sender, RoutedEventArgs e)
        {
            if(path == "")
            {
                System.Windows.MessageBox.Show("Specify path first.");

                return;
            }
            
            var filesToWorkWith = FilesHelper.GetAllFilesFromDirectory_Recursively(path);


            

            var thread = new Thread(new ThreadStart(() =>
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    Go_button.Content = "In progress...";
                    Go_button.IsEnabled = false;

                    Results_textBox.Text = "";
                }));


                var filePathsAndSizes = new Dictionary<string, long>();
                foreach (var filePah in filesToWorkWith)
                {
                    filePathsAndSizes[filePah] = new FileInfo(filePah).Length;
                }

                var groupedBySize = filePathsAndSizes.GroupBy(x => x.Value)
                    .ToDictionary(
                    x => x.Key,
                    x => x.Select(i => i.Key).ToList())
                    .Where(x => x.Value.Count > 1).ToArray();

                if (groupedBySize.Length == 0)
                {
                    NoDupes();

                    return;
                }




                var filePathsAndHashes = new Dictionary<string, string>();
                foreach(var groupedBySizeSubgroup in groupedBySize)
                {
                    foreach (var filePath in groupedBySizeSubgroup.Value)
                    {
                        filePathsAndHashes[filePath] = GetChecksum(filePath);
                    }
                }

                

                var groupedByChecksum = filePathsAndHashes.GroupBy(x => x.Value)
                    .ToDictionary(
                    x => x.Key,
                    x => x.Select(i => i.Key).ToList())
                    .Where(x => x.Value.Count > 1).ToArray();

                if (groupedByChecksum.Length == 0)
                {
                    NoDupes();

                    return;
                }

                var sb = new StringBuilder();

                foreach (var dupes in groupedByChecksum)
                {
                    sb.Append($"{dupes.Key}:{Environment.NewLine}{string.Join(Environment.NewLine, dupes.Value.ToArray())}{Environment.NewLine}{Environment.NewLine}");
                }

                Dispatcher.BeginInvoke(new Action(delegate
                {
                    Results_textBox.Text = sb.ToString();

                    Go_button.Content = "Go!";
                    Go_button.IsEnabled = true;
                }));

            }));

            thread.IsBackground = true;
            thread.Start();
        }

        private void NoDupes()
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                Results_textBox.Text = "No duplicates were found.";

                Go_button.Content = "Go!";
                Go_button.IsEnabled = true;
            }));
        }

        private static string GetChecksum(string file)
        {
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16 * 1024 * 1024))
            {
                //SHA256Managed sha = new SHA256Managed();
                //byte[] checksum = sha.ComputeHash(stream);
                var md5 = MD5.Create();
                var checksumBytes = md5.ComputeHash(stream);

                var chechsum = BitConverter.ToString(checksumBytes).Replace("-", "");

                return chechsum;
            }
        }
    }
}
