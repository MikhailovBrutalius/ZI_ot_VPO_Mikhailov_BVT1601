using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace AVTest
{
    class ResultObserver:IObserver<string>
    {
        private ListBox lbResult;
        private IDisposable unsubscription;
        public ResultObserver(ListBox lb)
        {
            lbResult = lb;
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { throw new Exception(); }

        delegate void addResult(string text);

        public void OnNext(string result)
        {
            lbResult.Dispatcher.Invoke(new addResult((s) => lbResult.Items.Add(s)), result);
        }

        public void Subscribe(AVScanner scanner)
        {
            unsubscription = scanner.Subscribe(this);
        }
    }
}
