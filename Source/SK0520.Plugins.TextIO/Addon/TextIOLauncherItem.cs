﻿using ContentTypeTextNet.Pe.Bridge.Models.Data;
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
    internal class TextIOLauncherItem : LauncherItemExtensionBase
    {
        #region define

        private string ListKey = string.Empty;
        private string HeadBaseKey = string.Empty;
        private string BodyBaseKey = string.Empty;

        #endregion

        public TextIOLauncherItem(ILauncherItemExtensionCreateParameter parameter, IPluginInformation pluginInformation, PluginBase plugin)
            : base(parameter, pluginInformation)
        {
            Plugin = plugin;
        }

        #region property

        private PluginBase Plugin { get; }

        #endregion

        #region function

        private string ToKey(string type, Guid scriptId)
        {
            return type + ":" + scriptId.ToString("D");
        }

        private string ToHeadKey(Guid scriptId) => ToKey("H", scriptId);

        private string ToMetaKey(Guid scriptId) => ToKey("M", scriptId);

        private string ToBodyKey(Guid scriptId) => ToKey("B", scriptId);

        private string ReadText(FileInfo file)
        {
            using var reader = file.OpenText();
            return reader.ReadToEnd();
        }

        private ScriptList GetScriptList(ILauncherItemAddonPersistence persistence)
        {
            ScriptList list = new ScriptList();

            if (persistence.Normal.TryGet(LauncherItemId, ListKey, out ScriptList? result))
            {
                list = result!;
            }

            return list;
        }

        public IEnumerable<ScriptHeadSetting> GetScriptHeads(ILauncherItemAddonPersistence persistence)
        {
            var scriptList = GetScriptList(persistence);
            var result = new List<ScriptHeadSetting>(scriptList.ScriptIds.Count);

            foreach (var scriptId in scriptList.ScriptIds)
            {
                if (persistence.Normal.TryGet(LauncherItemId, ToHeadKey(scriptId), out ScriptHeadSetting? head))
                {
                    result.Add(head!);
                }
            }

            return result;
        }

        public ScriptSetting AddScriptFile(FileInfo file)
        {
            var source = ReadText(file);
            var scriptLoader = new ScriptLoader(LoggerFactory);
            var scriptSetting = scriptLoader.LoadSource(source);

            ContextWorker.RunLauncherItemAddon(c =>
            {
                var scriptList = GetScriptList(c.Storage.Persistence);

                scriptList.ScriptIds.Add(scriptSetting.ScriptId);

                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ListKey, scriptList);
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ToHeadKey(scriptSetting.ScriptId), scriptSetting.Head);
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ToMetaKey(scriptSetting.ScriptId), scriptSetting.Meta);
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ToBodyKey(scriptSetting.ScriptId), scriptSetting.Body);

                return true;
            });

            return scriptSetting;
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
                () =>
                {
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
