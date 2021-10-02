using DbDiver.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DbDiver.Modules.ViewModels
{
    public class ProgressViewModel : BindableBase
    {
        private string _progressLabelContent;
        private IEventAggregator _eventAggregtor;

        public string ProgressLabelContent
        {
            get { return _progressLabelContent; }
            set { SetProperty(ref _progressLabelContent, value); }
        }

        public ProgressViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregtor = eventAggregator;
            _eventAggregtor.GetEvent<ProgramStartedEvent>().Subscribe(Started);
            _eventAggregtor.GetEvent<ProgramStoppedEvent>().Subscribe(Stopped);
        }

        public void Started()
        {
            ProgressLabelContent = "Started";
        }

        public void Stopped()
        {
            ProgressLabelContent = "Search completed";
        }
    }
}
