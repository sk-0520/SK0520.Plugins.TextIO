using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.Models.Data;
using ContentTypeTextNet.Pe.Bridge.Plugin;
using ContentTypeTextNet.Pe.Bridge.Plugin.Addon;
using ContentTypeTextNet.Pe.Bridge.Plugin.Preferences;
using ContentTypeTextNet.Pe.Embedded.Abstract;
using Jint.Native;
using Microsoft.Extensions.Logging;
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

        public ScriptMetaSetting GetMeta(Guid scriptId)
        {
            ScriptMetaSetting? result = null;

            ContextWorker.RunLauncherItemAddon(c =>
            {
                c.Storage.Persistence.Normal.TryGet<ScriptMetaSetting?>(c.LauncherItemId, ToMetaKey(scriptId), out result);
                return false;
            });

            if (result is null)
            {
                throw new KeyNotFoundException(scriptId.ToString());
            }

            return result;
        }

        private (string language, string source) GetScript(Guid scriptId)
        {
            string language = "javascript";
            string? source = null;

            ContextWorker.RunLauncherItemAddon(c =>
            {
                if(c.Storage.Persistence.Normal.TryGet(c.LauncherItemId, ToBodyKey(scriptId), out ScriptBodySetting?  result))
                {
                    source = result?.Source;
                }
                return false;
            });

            if(source is null)
            {
                throw new InvalidOperationException();
            }

            return (language, source);
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

        public void UpdateScript(ScriptSetting scriptSetting)
        {
            ContextWorker.RunLauncherItemAddon(c =>
            {
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ToHeadKey(scriptSetting.ScriptId), scriptSetting.Head);
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ToMetaKey(scriptSetting.ScriptId), scriptSetting.Meta);
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ToBodyKey(scriptSetting.ScriptId), scriptSetting.Body);

                return true;
            });
        }

        public void RemoveScript(Guid scriptId)
        {
            ContextWorker.RunLauncherItemAddon(c =>
            {
                var scriptList = GetScriptList(c.Storage.Persistence);

                scriptList.ScriptIds.Remove(scriptId);

                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ListKey, scriptList);
                c.Storage.Persistence.Normal.Delete(c.LauncherItemId, ToHeadKey(scriptId));
                c.Storage.Persistence.Normal.Delete(c.LauncherItemId, ToMetaKey(scriptId));
                c.Storage.Persistence.Normal.Delete(c.LauncherItemId, ToBodyKey(scriptId));

                return true;
            });
        }

        public async Task<ScriptSetting?> UpdateScriptIfNewVersionAsync(ScriptMetaSetting meta, Uri uri)
        {
            string source;
            if (uri.IsFile)
            {
                Logger.LogInformation("[{SCRIPT}] アップデート対象-ファイル: {TARGET}", meta.ScriptId, uri);
                source = await File.ReadAllTextAsync(uri.AbsolutePath);
            }
            else
            {
                Logger.LogInformation("[{SCRIPT}] アップデート対象-インターネット: {TARGET}", meta.ScriptId, uri);
                using var ua = HttpUserAgentFactory.CreateUserAgent();
                source = await ua.GetStringAsync(uri);
            }

            var scriptLoader = new ScriptLoader(LoggerFactory);

            var sourceHash = scriptLoader.ComputeHash(source);
            if (meta.HashKind == sourceHash.HashKind && meta.HashValue.SequenceEqual(sourceHash.HashValue))
            {
                Logger.LogInformation("[{SCRIPT}] 同一ハッシュ: <{KIND}> {VALUE}", meta.ScriptId, meta.HashKind, meta.HashValue);
                return null;
            }

            var scriptSetting = scriptLoader.LoadSource(source);
            var result = new ScriptSetting(
                scriptSetting.Head with
                {
                    ScriptId = meta.ScriptId,
                },
                scriptSetting.Meta with
                {
                    ScriptId = meta.ScriptId,
                    CreatedTimestamp = meta.CreatedTimestamp,
                },
                scriptSetting.Body with
                {
                    ScriptId = meta.ScriptId,
                }
            );

            return result;
        }

        public void ChangeOrder(Guid scriptId, bool isUp)
        {
            ContextWorker.RunLauncherItemAddon(c =>
            {
                var list = GetScriptList(c.Storage.Persistence);
                var index = list.ScriptIds.IndexOf(scriptId);
                list.ScriptIds.RemoveAt(index);
                if (isUp)
                {
                    list.ScriptIds.Insert(index - 1, scriptId);
                }
                else
                {
                    list.ScriptIds.Insert(index + 1, scriptId);
                }
                c.Storage.Persistence.Normal.Set(c.LauncherItemId, ListKey, list);

                return true;
            });
        }

        public Task RunScriptAsync(Guid scriptId, IReadOnlyDictionary<string, object?> parameters)
        {
            var script = GetScript(scriptId);
            var engine = new Jint.Engine()
            {
            };

            engine.Execute(script.source);

            return Task.CompletedTask;
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
