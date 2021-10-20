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
using System.Windows;

namespace DbDiver.Modules.ViewModels
{
    public class DiverParametersViewModel : BindableBase
    {
        private string _databasePath;

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
            Settings settings = new Settings(SearchParameters, DatabasePath);
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

        public void Dive()
        {
            _eventAggregator.GetEvent<ProgramStartedEvent>().Publish();
            if (!String.IsNullOrEmpty(DatabasePath))
            {
                var connectionString = $"Data Source = {DatabasePath};";
        
                foreach(var parameter in SearchParameters)
                {
                    try
                    {
                        bool paramFound = DbSearcher.CheckValueExist(parameter, connectionString);
                        if (paramFound)
                        {
                            parameter.Status = SearchStausMessages.FoundMessage;
                            parameter.LastFound = DateTime.Now.ToString();
                            if(parameter.FirstFound == null)
                            {
                                parameter.FirstFound = DateTime.Now.ToString();
                            }
                        }
                        else
                        {
                            parameter.Status = SearchStausMessages.NotFoundMessage;
                        }
                    }
                    catch
                    {
                        parameter.Status = SearchStausMessages.ErrorMessage;
                    }
                }
            }
            RaisePropertyChanged("SearchParameters");
            _eventAggregator.GetEvent<ProgramStoppedEvent>().Publish();
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
    }
}
