using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows;
namespace AVTest
{
    class AVScanner : IObserver<AVScanObject>, IObservable<string>
    {
        List<AVEntryTree> avBases;
        List<IObserver<string>> observers;
        List<string> viruses;
        public AVScanner()
        {
            avBases = new List<AVEntryTree>();
            observers = new List<IObserver<string>>();
            viruses = new List<string>();
        }

        public void OnCompleted()
        {
            return;
        }

        public void OnError(Exception error)
        {
            throw new Exception();
        }

        public void OnNext(AVScanObject scanObj)
        {
            if (scanObj == null) return;
            byte[] firstBytes = new byte[AVEntry.WINDOW];
            long length = scanObj.Length;
            int offset = 0;
            do
            {
                firstBytes = scanObj.GetBytes(offset, AVEntry.WINDOW);
                AVEntryTreeNode node;
                foreach (AVEntryTree avBase in avBases)
                {
                    do
                    {
                        node = avBase.findNode(Encoding.ASCII.GetString(firstBytes));
                        if (node == null) break;
                        if (node.getOffset()[0] <= offset && offset <= node.getOffset()[1])
                        {
                            MD5 md5 = MD5.Create();
                            byte[] hash = md5.ComputeHash(scanObj.GetBytes(offset, node.getSignatureLength()));
                            if (hashComp(hash, node.getHash()))
                            {
                                string name = node.getName().Split(':')[0];
                                viruses.Add(name + " in " + scanObj.getPath());
                                foreach (IObserver<string> observer in observers)
                                    observer.OnNext(viruses.Last());
                                scanObj.Close();
                                return;
                            }
                        }
                    } while (node != null);
                }
                offset++;
            } while (offset < length - AVEntry.WINDOW);
            scanObj.Close();
        }

        private bool hashComp(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length) return false;
            for (int i = 0; i<hash1.Length; i++)
            {
                if (hash1[i] != hash2[i]) return false;
            }
            return true;
        }

        public void addBase(AVEntryTree avBase)
        {
            avBases.Add(avBase);
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                foreach (string virus in viruses)
                    observer.OnNext(virus);
            }
            return new ScannerUnsubscriber(observers, observer);
        }


        class ScannerUnsubscriber : IDisposable
        {
            private List<IObserver<string>> _observers;
            private IObserver<string> _observer;

            public ScannerUnsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
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
