using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbDiver.Core.Events;
using System.Collections.ObjectModel;
using System.Threading;

namespace DbDiver.Modules.Log.ViewModels
{
    public class LogViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;

        public ObservableCollection<string> LogItems { set; get; } = new ObservableCollection<string>();

        public LogViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<SearchStartedEvent>().Subscribe(Started);
            _eventAggregator.GetEvent<SearchFinishedEvent>().Subscribe(Stopped);
            _eventAggregator.GetEvent<ValueFoundEvent>().Subscribe(ValueFound);
        }

        public void Started(int paramCount)
        {
            
            var uiContext = SynchronizationContext.Current;
            uiContext.Send(x =>
            {
                LogItems.Clear();
                LogItems.Add($"Diving started at {DateTime.Now} with {paramCount} item(s) to process ");
                RaisePropertyChanged("LogItems");
            }, null
            );

        }

        public void Stopped()
        {
            LogItems.Add($"Diving finished at {DateTime.Now}");
            RaisePropertyChanged("LogItems");
        }


        public void ValueFound(string presenation)
        {
            LogItems.Add(presenation);
            RaisePropertyChanged("LogItems");
        }
    }
}
