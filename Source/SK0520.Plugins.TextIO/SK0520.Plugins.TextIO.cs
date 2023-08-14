using System;
using System.Collections.Generic;
using System.Text;
using ContentTypeTextNet.Pe.Bridge.Plugin;
using ContentTypeTextNet.Pe.Bridge.Plugin.Addon;
using ContentTypeTextNet.Pe.Bridge.Plugin.Preferences;
using ContentTypeTextNet.Pe.Embedded.Abstract;
using SK0520.Plugins.TextIO.Addon;

namespace SK0520.Plugins.TextIO
{
    public class TextIO: PluginBase, IAddon// IAddon, ITheme, IPreferences
    {
        #region variable

        private TextIOAddon _addon;

        #endregion


        public TextIO(IPluginConstructorContext pluginConstructorContext)
            : base(pluginConstructorContext)
        {
            this._addon = new TextIOAddon(pluginConstructorContext, this);
        }

        #region PluginBase

        internal override AddonBase Addon => this._addon;

        protected override void InitializeImpl(IPluginInitializeContext pluginInitializeContext)
        { }

        protected override void FinalizeImpl(IPluginFinalizeContext pluginFinalizeContext)
        { }


        #endregion

    }
}
