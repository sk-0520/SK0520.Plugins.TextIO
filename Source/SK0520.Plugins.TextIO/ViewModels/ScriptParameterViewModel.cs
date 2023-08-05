using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.ViewModels;
using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.ViewModels
{
    public class ScriptParameterViewModel : ViewModelSkeleton
    {
        public ScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            Parameter = parameter;
        }

        #region property

        private ScriptParameter Parameter { get; }
        #endregion
    }
}
