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
    public class ScriptHeadViewModel: ViewModelSkeleton, IScriptId
    {
        #region variable

        private ScriptUpdateStatus _scriptUpdateStatus;

        #endregion

        public ScriptHeadViewModel(ScriptHeadSetting headSetting, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory) 
            :base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            HeadSetting = headSetting;
            var factory = new ScriptParameterViewModelFactory(skeletonImplements, dispatcherWrapper, loggerFactory);
            ParameterCollection = new ObservableCollection<ScriptParameterViewModelBase>(HeadSetting.Parameters.Select(a => factory.Create(a)));
        }

        #region property

        private ScriptHeadSetting HeadSetting { get; }

        public string Name => HeadSetting.Name;

        public ObservableCollection<ScriptParameterViewModelBase> ParameterCollection { get; }

        public bool HasParameters => ParameterCollection.Any();

        public ScriptUpdateStatus ScriptUpdateStatus
        {
            get => this._scriptUpdateStatus;
            set => SetProperty(ref this._scriptUpdateStatus, value);
        }

        #endregion

        #region IScriptId

        public Guid ScriptId => HeadSetting.ScriptId;

        #endregion
    }
}
