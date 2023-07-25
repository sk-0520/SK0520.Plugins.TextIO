using ContentTypeTextNet.Pe.Bridge.Plugin;
using ContentTypeTextNet.Pe.Bridge.Plugin.Addon;
using ContentTypeTextNet.Pe.Embedded.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Addon
{
    internal class TextIOAddon : AddonBase
    {
        public TextIOAddon(IPluginConstructorContext pluginConstructorContext, PluginBase plugin)
            : base(pluginConstructorContext, plugin)
        { }

        #region AddonBase

        protected override IReadOnlyCollection<AddonKind> SupportedKinds { get; } = new[] {
            AddonKind.LauncherItem,
        };

        protected internal override void Load(IPluginLoadContext pluginLoadContext)
        { }

        protected internal override void Unload(IPluginUnloadContext pluginUnloadContext)
        { }

        public override ILauncherItemExtension CreateLauncherItemExtension(ILauncherItemExtensionCreateParameter parameter)
        {
            return new TextIOLauncherItem(parameter, PluginInformation, Plugin);
        }

        #endregion
    }
}
