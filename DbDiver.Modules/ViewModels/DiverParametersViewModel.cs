using DbDiver.Business;
using DbDiver.Core;
using DbDiver.Core.Events;
using DbDiver.DAL;
using DbDiver.Modules.Exceptions;
using DbDiver.Modules.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DbDiver.Modules.ViewModels
{
    public class DiverParametersViewModel : BindableBase
    {
        private string _databasePath;
        private int _selectedDatabaseIdx;
        public int SelectedDatabaseIdx
        {
            get { return _selectedDatabaseIdx; }
            set
            {
                SetProperty(ref _selectedDatabaseIdx, value);
            }
        }
        
        public string DatabasePath
        {
            get { return _databasePath; }
            set { 
                SetProperty(ref _databasePath, value);
                DiveCommand.RaiseCanExecuteChanged();
            }
        }

        private string _tableName;
        public string TableName
        {
            get { return _tableName; }
            set { 
                SetProperty(ref _tableName, value);
                AddItemCommand.RaiseCanExecuteChanged();
            }
        }
        
        private string _searchItem;
        public string SearchItem
        {
            get { return _searchItem; }
            set { 
                SetProperty(ref _searchItem, value); 
                AddItemCommand.RaiseCanExecuteChanged();
            }
        }
        
        private string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
            set { 
                SetProperty(ref _columnName, value);
                AddItemCommand.RaiseCanExecuteChanged();
            }
        }
        
        private string _descripion;
        private readonly IEventAggregator _eventAggregator;

        public string Description
        {
            get { return _descripion; }
            set
            {
                SetProperty(ref _descripion, value);
                AddItemCommand.RaiseCanExecuteChanged();
            }
        }

        public DiverParametersViewModel(IEventAggregator eventAggregator)
        {
            AddItemCommand = new DelegateCommand(AddItem, CanBeAdded);
            DiveCommand = new DelegateCommand(Dive, CanPerformDive);
            SearchParameters = new ObservableCollection<DbSearchParameter>();
            BrowseCommand = new DelegateCommand(BrowseSqlFile);
            SaveItemsCommand = new DelegateCommand(SaveItems);
            LoadItemsCommand = new DelegateCommand(LoadItems);
            Settings settings = new Settings();
            try
            {
                settings.Load();
                SearchParameters = settings.Parameters;
                DatabasePath = settings.DatabasePath;
                SelectedDatabaseIdx = settings.SelectedDatabaseIdx;

            }
            catch (SettingsNotFoundException)
            {
                MessageBox.Show("Settings file not found");
            }
            catch
            {
                MessageBox.Show("Settings loading error");
            }
            SetStatusNotSearched();
            _eventAggregator = eventAggregator;
        }

        ~DiverParametersViewModel()
        {
            Settings settings = new Settings(SearchParameters, DatabasePath, SelectedDatabaseIdx);
            settings.Save();
            
        }

        private void AddItem()
        {
            SearchParameters.Add(new DbSearchParameter(ColumnName, TableName, SearchItem, Description, null, null));
        }


        private void BrowseSqlFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            DatabasePath = ofd.FileName;
            SetStatusNotSearched();
        }

        private bool CanBeAdded()
        {
            return ColumnName?.Length >0 && TableName?.Length>0 && SearchItem?.Length>0 && Description?.Length > 0; 
        }


        public DelegateCommand AddItemCommand { get; private set; }

        public DelegateCommand DiveCommand { get; private set; }

        public DelegateCommand BrowseCommand { get; private set; }

        public DelegateCommand LoadItemsCommand { get;private set; }
        public DelegateCommand SaveItemsCommand { get; private set; }

        public ObservableCollection<DbSearchParameter> SearchParameters { set; get; }
        public ObservableCollection<string> LogItems { set; get; } = new ObservableCollection<string>();

        public async void Dive()
        {
           await Task.Run(() =>
           {
               _eventAggregator.GetEvent<SearchStartedEvent>().Publish(SearchParameters.Count);
               int foundCount = 0;
               if (!String.IsNullOrEmpty(DatabasePath))
               {
                   AddLogMessage($"program started");

                   IDatabaseItemsExtractor databaseItemsExtractor = null;
                   try
                   {
                       switch (SelectedDatabaseIdx)
                       {
                           case 0:
                               {
                                   databaseItemsExtractor = new SqliteItemsExtractor($"Data Source = {DatabasePath};"); break;
                               };
                           case 1:
                               {
                                   var database = Path.GetFileName(DatabasePath);
                                   var server = Path.GetDirectoryName(DatabasePath);
                                   databaseItemsExtractor = new MSSqlItemsExtractor($"Server={server};Database={database};");
                                   break;
                               };
                       };
                       var databaseSearcher = new DatabaseSearcher(_eventAggregator, databaseItemsExtractor);
                       foundCount = databaseSearcher.InspectValues(SearchParameters, AddLogMessage);
                   }
                   catch (Exception exc)
                   {
                       MessageBox.Show($"Error database initialization: {exc.Message}");
                       AddLogMessage($"Error database initialization: {exc.Message}");
                   }
                  
                   AddLogMessage($"program finished with {foundCount} found values");
               }
               RaisePropertyChanged("SearchParameters");
               _eventAggregator.GetEvent<SearchFinishedEvent>().Publish();
           });

        }
        public bool CanPerformDive()
        {
            return DatabasePath.Length > 0;
        }

        private void LoadItems()
        {
            SearchParameters = new ObservableCollection<DbSearchParameter>(ItemsLoader.LoadItems());
            RaisePropertyChanged("SearchParameters");
        }

        private void SaveItems()
        {
            ItemsLoader.SaveItems(SearchParameters);
        }

        public void SetStatusNotSearched()
        {
            foreach (var v in SearchParameters)
            {
                v.Status = "Not searched";
            }
        }

        public void AddLogMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(
            () =>
            {
                string dot = message.EndsWith(".") ? string.Empty : ".";
                LogItems.Add($"{DateTime.Now}: {message}{dot}");
                RaisePropertyChanged("LogItems");
            }
            );        

        }
    }
}
