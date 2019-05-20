using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace AVTest
{
    class AVScanObjFactory:IObserver<string>,IObservable<AVScanObject>
    {
        private IDisposable unsubscription;
        private List<IObserver<AVScanObject>> observers;
        private List<AVScanObject> objects;

        public AVScanObjFactory()
        {
            objects = new List<AVScanObject>();
            observers = new List<IObserver<AVScanObject>>();
        }

        public void Subscribe(AVFileFinder finder)
        {
            unsubscription = finder.Subscribe(this);
        }

        public void OnCompleted()
        {
            if (unsubscription != null) unsubscription.Dispose();
        }

        public void OnError(Exception error)
        {
            throw new Exception();
        }

        public void OnNext(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            FileInfo fi = new FileInfo(path);      
            Mutex m = new Mutex(false, "m" + filename);
            filename += Convert.ToString(objects.Count);
            MemoryMappedFile mappedFile = null ;
            m.WaitOne();
            try
            {
                mappedFile = MemoryMappedFile.CreateFromFile(path, FileMode.Open, filename, 0);
            }
            catch (IOException) { return; }
            MemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
            uint offsetPE = accessor.ReadUInt32(60);                     // Считываем сдвиг заголовка PE
            AVScanObject obj = null;
            // Проверяем размер файла
            if (offsetPE <= accessor.Capacity)
            {
                byte[] peSignature = new byte[4];
                accessor.ReadArray(offsetPE, peSignature, 0, 4);         // Считываем сигнатуру
                if (Encoding.ASCII.GetString(peSignature) == "PE\0\0")   // Для PE-файла должно быть равно PE\0\0
                {
                    ushort magic = accessor.ReadUInt16(offsetPE+24);     // Считываем поле magic определяющее вид PE-заголовка
                    uint peHeaderSize = 0;
                    if (magic == 267) peHeaderSize = 248;                // Для PE32
                    else peHeaderSize = 264;                             // Для PE32+
                    uint sectionsNum = accessor.ReadUInt16(offsetPE + 6);// Считываем количество секций
                    uint pFirstSection = offsetPE + peHeaderSize;
                    for (uint i = 0; i<sectionsNum; i++)
                    {
                        uint pCurSection = pFirstSection + i * 40;
                        uint characteristic = accessor.ReadUInt32(pCurSection + 36);
                        uint executeFlag = 0x20000000;
                        // Проверяем содержимое поля характеристики
                        // Eсли сегмент исполняемый, то создаем AVScanObject с этим сегментом
                        if ((characteristic & executeFlag) == executeFlag )
                        {
                            uint pDataStart  = accessor.ReadUInt32(pCurSection + 20);
                            uint pDataLength = accessor.ReadUInt32(pCurSection + 16);
                            MemoryMappedViewAccessor sectionAccessor = mappedFile.CreateViewAccessor(pDataStart, pDataLength, MemoryMappedFileAccess.Read);
                            obj = new AVScanObject(sectionAccessor, mappedFile, path);
                        }
                    }

                }
            }
            m.ReleaseMutex();
            // По окончанию проверки файла, добавляем в список obj, который, если файл не исполняемый равен null
            objects.Add(obj);
            foreach (IObserver<AVScanObject> observer in observers)     // И оповещаем всех слушателей
                observer.OnNext(obj);

        }

        public IDisposable Subscribe(IObserver<AVScanObject> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                foreach (AVScanObject obj in objects)
                    observer.OnNext(obj);
            }
            return new FactoryUnsubscriber(observers, observer);
        }

        class FactoryUnsubscriber:IDisposable
        {
            private List<IObserver<AVScanObject>> _observers;
            private IObserver<AVScanObject> _observer;

            public FactoryUnsubscriber(List<IObserver<AVScanObject>> observers, IObserver<AVScanObject> observer)
            {
                _observer = observer;
                _observers = observers;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer))
                    _observers.Remove(_observer); 
            }
        }
    }
}
