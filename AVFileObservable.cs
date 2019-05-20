using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVTest
{
    class AVFileObservable : IObservable<string>
    {
        public List<IObserver<string>> observers;
        public List<string> files;
        public AVFileObservable()
        {
            observers = new List<IObserver<string>>();
            files = new List<string>();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                foreach (string file in files)
                {
                    observer.OnNext(file);
                }
            }
            return new Unsubscriber(observers, observer);
        }

        public void addFile(string path)
        {
            if (!files.Contains(path))
            {
                files.Add(path);
            }
        }

        public void updateObservers(string path)
        {
            foreach (IObserver<string> observer in observers)
            {
                observer.OnNext(path);
            }
        }

        class Unsubscriber : IDisposable
        {
            public List<IObserver<string>> _observers;
            public IObserver<string> _observer;
            public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
            {
                _observers = observers;
                _observer = observer;
            }
            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }

        
    }
}
