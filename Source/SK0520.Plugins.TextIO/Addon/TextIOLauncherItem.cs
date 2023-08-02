using ContentTypeTextNet.Pe.Bridge.Models.Data;
using ContentTypeTextNet.Pe.Bridge.Plugin;
using ContentTypeTextNet.Pe.Bridge.Plugin.Addon;
using ContentTypeTextNet.Pe.Bridge.Plugin.Preferences;
using ContentTypeTextNet.Pe.Embedded.Abstract;
using SK0520.Plugins.TextIO.Models;
using SK0520.Plugins.TextIO.Models.Data;
using SK0520.Plugins.TextIO.ViewModels;
using SK0520.Plugins.TextIO.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Addon
{
    public class TextIOLauncherItem : LauncherItemExtensionBase
    {
        public TextIOLauncherItem(ILauncherItemExtensionCreateParameter parameter, IPluginInformation pluginInformation, PluginBase plugin)
            : base(parameter, pluginInformation)
        {
            Plugin = plugin;
        }

        #region property

        private PluginBase Plugin { get; }

        #endregion

        #region function

        private string ReadText(FileInfo file)
        {
            using var reader = file.OpenText();
            return reader.ReadToEnd();
        }

        public void AddScriptFile(FileInfo file)
        {
            var source = ReadText(file);
            var scriptLoader = new ScriptLoader();
            var script = scriptLoader.LoadSource(source);

            ContextWorker.RunLauncherItemAddon(c =>
            {
                ScriptList? list = null;
                if (!c.Storage.Persistence.Normal.TryGet(c.LauncherItemId, string.Empty, out list))
                {
                    list = new ScriptList();
                }
                Debug.Assert(list is not null);

                list.ScriptIds.Add(script.ScriptId);

                c.Storage.Persistence.Normal.Set(c.LauncherItemId, string.Empty, list);
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, "H:" + script.ScriptId.ToString("D"), script.Head);
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, "B:" + script.ScriptId.ToString("D"), script.Body);
            });

        }

        #endregion

        #region LauncherItemExtensionBase

        public override bool CustomDisplayText => false;

        public override string DisplayText => PluginInformation.PluginIdentifiers.PluginName;

        public override bool CustomLauncherIcon => throw new NotSupportedException();

        public override bool SupportedPreferences => false;

        public override void ChangeDisplay(LauncherItemIconMode iconMode, bool isVisible, object callerObject)
        { }

        public override ILauncherItemPreferences CreatePreferences(ILauncherItemAddonContext launcherItemAddonContext)
        {
            throw new NotSupportedException();
        }

        public override void Execute(string? argument, ICommandExecuteParameter commandExecuteParameter, ILauncherItemExtensionExecuteParameter launcherItemExtensionExecuteParameter, ILauncherItemAddonContext launcherItemAddonContext)
        {
            var viewModel = new TextIOLauncherItemViewModel(this, launcherItemAddonContext, SkeletonImplements, DispatcherWrapper, LoggerFactory);
            var view = new TextIOLauncherItemWindow()
            {
                DataContext = viewModel,
            };

            launcherItemExtensionExecuteParameter.ViewSupporter.RegisterWindow(
                view,
                () => !viewModel.IsRunning,
                () => {
                    view.DataContext = null;
                    viewModel.Dispose();
                }
            );
        }

        public override object GetIcon(LauncherItemIconMode iconMode, in IconScale iconScale)
        {
            return Plugin.GetIcon(ImageLoader, iconScale);
        }

        #endregion
    }
}
