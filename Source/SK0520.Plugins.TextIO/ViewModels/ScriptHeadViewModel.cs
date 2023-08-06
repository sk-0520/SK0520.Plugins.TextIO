using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.ViewModels;
using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.ViewModels
{
    public class ScriptHeadViewModel: ViewModelSkeleton
    {
        public ScriptHeadViewModel(ScriptHeadSetting headSetting, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory) 
            :base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            HeadSetting = headSetting;
            ParameterCollection = new ObservableCollection<ScriptParameterViewModel>(HeadSetting.Parameters.Select(a => new ScriptParameterViewModel(a, skeletonImplements, dispatcherWrapper, loggerFactory)));
        }

        #region property

        private ScriptHeadSetting HeadSetting { get; }

        public string Name => HeadSetting.Name;

        public ObservableCollection<ScriptParameterViewModel> ParameterCollection { get; }

        public bool ParameterIsEmpty => ParameterCollection.Count == 0;

        #endregion
    }
}
