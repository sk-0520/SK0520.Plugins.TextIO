using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.ViewModels;
using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Addon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.ViewModels
{
    internal class TextIOLauncherItemViewModel : ViewModelSkeleton
    {
        #region variable

        private bool _isRunning = false;

        #endregion

        public TextIOLauncherItemViewModel(TextIOLauncherItem item, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            Item = item;
        }

        #region property

        private TextIOLauncherItem Item { get; }

        public bool IsRunning
        {
            get => this._isRunning;
            set => SetProperty(ref this._isRunning, value);
        }

        #endregion  
    }
}
