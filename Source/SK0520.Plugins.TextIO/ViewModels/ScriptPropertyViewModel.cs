using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.ViewModels;
using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.ViewModels
{
    public class ScriptPropertyViewModel : ViewModelSkeleton, IScriptId
    {
        public ScriptPropertyViewModel(ScriptHeadSetting headSetting,ScriptMetaSetting metaSetting, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            if(headSetting.ScriptId != metaSetting.ScriptId)
            {
                throw new ArgumentException(nameof(IScriptId.ScriptId));
            }

            HeadSetting = headSetting;
            MetaSetting = metaSetting;
        }

        #region property

        private ScriptHeadSetting HeadSetting { get; }
        private ScriptMetaSetting MetaSetting { get; }

        public string Name => HeadSetting.Name;
        public string UpdateUri => MetaSetting.UpdateUri;
        public DateTime CreatedTimestamp => MetaSetting.CreatedTimestamp;
        public DateTime UpdatedTimestamp  => MetaSetting.UpdatedTimestamp;
        public string HashKind => MetaSetting.HashKind;
        public string HashValue => BitConverter.ToString(MetaSetting.HashValue);


        #endregion

        #region IScriptId

        public Guid ScriptId  => HeadSetting.ScriptId;

        #endregion
    }
}
