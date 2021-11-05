using DbDiver.Core;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using DbDiver.Modules.Log.Views;

namespace DbDiver.Modules.Log
{
    public class LogModule : IModule
    {
        IRegionManager _regionManager;

        public LogModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.LogRegion, typeof(LogView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
        }
    }
}