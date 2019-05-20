using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using winforms = System.Windows.Forms;
using System.IO;

namespace AVTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AVEntryTree entryList;
        public MainWindow()
        {
            InitializeComponent();
            AVLoader.initDB();
            entryList = AVLoader.loadEntries();
            if (entryList.root != null)
            {
                foreach (string entry in entryList.getAllNames(entryList.root))
                    lbNames.Items.Add(entry);
            }
        }


        private void update_ListBox()
        {
            lbNames.Items.Clear();
            if (entryList.root == null) return;
            foreach (string entry in entryList.getAllNames(entryList.root))
                lbNames.Items.Add(entry);
        }

        private void addEntry_Click(object sender, RoutedEventArgs e)
        {
            AVEntry newEntry = new AVEntry(tbTitle.Text);
            string[] offset = tbOffset.Text.Split(';');
            if (offset.Count() != 2) return;
            newEntry.setSignatureInfo(Convert.ToInt16(offset[0]), Convert.ToInt16(offset[1]), tbSignature.Text, tbTypes.Text);
            entryList.insertNode(newEntry);
            update_ListBox();
        }

        private void lbNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbNames.SelectedItem == null) return;
            string key = lbNames.SelectedItem.ToString();
            key = key.Split(':')[1];
            AVEntryTreeNode node = entryList.findNode(key);
            if (node != null)
            {
                tbInfo.Text = node.getInfo();
            }
        }

        private void saveList_Click(object sender, RoutedEventArgs e)
        {
            AVLoader.saveEntries(entryList);
        }

        private void check(string path)
        {

            AVFileFinder finder = new AVFileFinder(path);
            AVScanObjFactory factory = new AVScanObjFactory();
            AVScanner scanner = new AVScanner();
            scanner.addBase(entryList);
            ResultObserver results = new ResultObserver(lbResultCheck);
            factory.Subscribe(finder);
            factory.Subscribe(scanner);
            results.Subscribe(scanner);
            finder.StartSearch();
        }

        private void checkFiles_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "All Files (*.*)|*.*";
            var result = ofd.ShowDialog();
            if (result == true)
            {
                string path = ofd.FileName;
                tbFilenameCheck.Text = path;
                check(path);
            }
        }

        private void checkDirectory_Click(object sender, RoutedEventArgs e)
        {
            winforms.FolderBrowserDialog fbd = new winforms.FolderBrowserDialog();
            if (fbd.ShowDialog() == winforms.DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                tbFilenameCheck.Text = path;
                check(path);
            }
        }

        private void addWatch_Click(object sender, RoutedEventArgs e)
        {
            winforms.FolderBrowserDialog fbd = new winforms.FolderBrowserDialog();
            if (fbd.ShowDialog() == winforms.DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                tbDirNameWatch.Text = path;
                lbDirWatch.Items.Add(path);
                AVFileWatcher watcher = new AVFileWatcher();
                AVScanObjFactory factory = new AVScanObjFactory();
                AVScanner scanner = new AVScanner();
                scanner.addBase(entryList);
                ResultObserver results = new ResultObserver(lbResultWatch);
                watcher.Subscribe(factory);
                factory.Subscribe(scanner);
                results.Subscribe(scanner);
                watcher.AddDirectory(path);
            }
        }
    }
}
