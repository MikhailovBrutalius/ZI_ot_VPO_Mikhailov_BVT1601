using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Threading;
namespace AVTest
{
    class AVFileWatcher : AVFileObservable
    {
        private List<string> directories;
        private List<FileSystemWatcher> watchers;

        public AVFileWatcher()
        {
            directories = new List<string>();
            watchers = new List<FileSystemWatcher>();
        }

        private void unzipAllZipFiles(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            FileInfo[] zips = di.GetFiles("*.zip", SearchOption.AllDirectories);
            foreach (FileInfo zip in zips )
            {
                string extractPath = zip.FullName.Split('.')[0];
                if (!Directory.Exists(extractPath))
                {
                    ZipFile.ExtractToDirectory(zip.FullName, extractPath);
                }
            }
        }

        private void AddDirectoryWithoutScan(string path)
        {
            if (!directories.Contains(path))
            {
                directories.Add(path);
                FileSystemWatcher watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
                                       | NotifyFilters.DirectoryName;
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);
                watcher.EnableRaisingEvents = true;
                watchers.Add(watcher);
            }
        }

        public bool AddDirectory(string path)
        {
            if (!directories.Contains(path))
            {
                unzipAllZipFiles(path);
                // Создаем поисковик, который найдет все испоняемые файлы
                AVFileFinder fileFinder = new AVFileFinder(path);
                foreach (var observer in observers)
                    fileFinder.Subscribe(observer);
                fileFinder.StartSearch();
                // Настраиваем отслеживание для данной папки
                AddDirectoryWithoutScan(path);
                // Выполняем поиск всех папок внутри данной
                string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                // И настраиваем отслеживание на них
                foreach (string dir in dirs)
                    AddDirectoryWithoutScan(dir);
                return true;
            }
            return false;
        }

        private void OnChanged(object sourse, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            //Mutex checkExtracting = new Mutex(false, "extracting");
            //if (!checkExtracting.WaitOne(1))
            //{
            //    if (Directory.Exists(path) && e.ChangeType == WatcherChangeTypes.Created) AddDirectoryWithoutScan(path);
            //    return;
            //}
            if (Directory.Exists(path) && e.ChangeType == WatcherChangeTypes.Created)
            {
                    // Если внутри папки создан файл, то вызываем функцию, которая найдет новый файл
                    AddDirectory(path);
                   
            }
            else if (File.Exists(path))
            {             
                // Если изменился как-либо файл внутри папок, тогда оповещаем слушателей
                updateObservers(path);
            }
            //checkExtracting.ReleaseMutex();
        }

        private void OnRenamed(object sourse, RenamedEventArgs e)
        {
            string path = e.FullPath;
            if (Directory.Exists(path))
            {
                // При переименовании папки обновляем слушателей
                AddDirectoryWithoutScan(path);
                // Удаляем из списка директорий старое название
                directories.Remove(e.OldFullPath);
                bool flag = true;
                for (int i = 0; i < watchers.Count && flag; i++)
                {
                    if (watchers[i].Path == e.OldFullPath)
                    {
                        // Удаляем из списка смотрителя за папкой с прежним названием
                        watchers.RemoveAt(i);
                        flag = false;
                    }
                }
            }
        }
    }
}

