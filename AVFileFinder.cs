using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
namespace AVTest
{
    class AVFileFinder : AVFileObservable
    {
        private string startPath;
        public AVFileFinder(string path)
        {
            startPath = path;
        }

        public void StartSearch()
        {
            // Поиск выполняется в другом потоке
            Thread search = new Thread(Search);
            search.Name = "Search";
            search.Start();
        }

        private void findExecutables(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            FileInfo[] allFiles = di.GetFiles("*.exe", SearchOption.AllDirectories);
            foreach (FileInfo file in allFiles)
            {
                string fullPath = file.FullName;
                addFile(fullPath);
                updateObservers(fullPath);
            }

        }

        private void findZip(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            FileInfo[] allFiles = di.GetFiles("*.zip", SearchOption.AllDirectories);
            foreach (FileInfo file in allFiles)
            {
                string extractPath = file.FullName.Split('.')[0];
                if (!Directory.Exists(extractPath))
                {
                    Mutex extracting = new Mutex(false, "extracting");
                    extracting.WaitOne();
                    ZipFile.ExtractToDirectory(file.FullName, extractPath);
                    findExecutables(extractPath);   
                    extracting.ReleaseMutex();
                }
            }
        }

        private void Search()
        {
            if (File.Exists(startPath))         // Если это файл, то проверяем расширение и оповещаем слушателей
            {
                FileInfo fi = new FileInfo(startPath);
                if (fi.Extension == ".exe")
                {
                    addFile(fi.FullName);
                    updateObservers(fi.FullName);
                }
                else if (fi.Extension == ".zip")
                {
                    string extractPath = fi.FullName.Split('.')[0];
                    if (!Directory.Exists(extractPath))
                    {
                        ZipFile.ExtractToDirectory(startPath, extractPath);
                        findExecutables(extractPath);
                    }
                }
            }
            else                                // Если это папка, то ищем в ней все .exe файлы и оповещаем слушателей
            {
                findExecutables(startPath);
                findZip(startPath);

                
            }


        }
    }
}
