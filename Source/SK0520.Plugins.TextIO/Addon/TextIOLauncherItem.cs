using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.Models.Data;
using ContentTypeTextNet.Pe.Bridge.Plugin;
using ContentTypeTextNet.Pe.Bridge.Plugin.Addon;
using ContentTypeTextNet.Pe.Bridge.Plugin.Preferences;
using ContentTypeTextNet.Pe.Embedded.Abstract;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Runtime;
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
using System.Text.Json;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Addon
{
    internal class TextIOLauncherItem : LauncherItemExtensionBase
    {
        #region define

        private string ListKey = string.Empty;
        private string HeadBaseKey = string.Empty;
        private string BodyBaseKey = string.Empty;

        private string DefaultEntryFunctionName = "handler";

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

        private ScriptMetaSetting GetMetaCore(Guid scriptId, ILauncherItemAddonPersistence persistence)
        {
            persistence.Normal.TryGet<ScriptMetaSetting?>(LauncherItemId, ToMetaKey(scriptId), out var result);
            if (result is null)
            {
                throw new KeyNotFoundException(scriptId.ToString());
            }

            return result;
        }

        public ScriptMetaSetting GetMeta(Guid scriptId)
        {
            ScriptMetaSetting? result = null;

            ContextWorker.RunLauncherItemAddon(c =>
            {
                result = GetMetaCore(scriptId, c.Storage.Persistence);
                return false;
            });

            Debug.Assert(result is not null);

            return result;
        }

        private (string language, string source) GetScriptCore(Guid scriptId, ILauncherItemAddonPersistence persistence)
        {
            string language = "javascript";
            string? source = null;

            if (persistence.Normal.TryGet(LauncherItemId, ToBodyKey(scriptId), out ScriptBodySetting? result))
            {
                source = result?.Source;
            }

            if (source is null)
            {
                throw new InvalidOperationException();
            }

            return (language, source);
        }

        private (string language, string source) GetScript(Guid scriptId)
        {
            string language = string.Empty;
            string source = string.Empty;

            ContextWorker.RunLauncherItemAddon(c =>
            {
                (language, source) = GetScriptCore(scriptId, c.Storage.Persistence);
                return false;
            });

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

        public JsValue InjectSource(Engine engine, Guid scriptId, string scriptName, string source, string entryFunctionName)
        {
            var handler = engine
                .SetValue(
                    "Pe",
                    new
                    {
                        logger = new ScriptLogger(scriptId, scriptName, LoggerFactory),
                    }
                )
                .Execute(source)
                .GetValue(entryFunctionName)
            ;

            return handler;
        }

        public Task<ScriptResponse> RunScriptAsync(Guid scriptId, string input, IReadOnlyDictionary<string, object?> parameters)
        {
            string scriptName = string.Empty;

            var script = GetScript(scriptId);

            var entryFunctionName = DefaultEntryFunctionName;

            var engine = new Jint.Engine();

            var beginTimestamp = DateTime.UtcNow;

            try
            {
                var handler = InjectSource(engine, scriptId, scriptName, script.source, entryFunctionName);
                if (handler == JsValue.Undefined)
                {
                    throw new KeyNotFoundException(entryFunctionName);
                }

                var args = JsValue.FromObject(engine, new
                {
                    input = input,
                    parameters = parameters,
                });

                var result = handler.Invoke(args);
                var endTimestamp = DateTime.UtcNow;

                if (!(result.IsString() || result.IsObject() || result.IsNumber() || result.IsBoolean()) || result.IsArray())
                {
                    throw new Exception("undefined " + result);
                }

                var resultObject = result.ToObject();
                if (resultObject is System.Dynamic.ExpandoObject dynamicValue)
                {
                    var map = (IDictionary<string, object?>)dynamicValue;

                    if (map.TryGetValue("kind", out var resultRawKind) && resultRawKind is string resultKind)
                    {
                        if (Enum.TryParse(resultKind, true, out ScriptResultKind kind))
                        {
                            if (map.TryGetValue("data", out var resultRawData))
                            {
                                switch (kind)
                                {
                                    case ScriptResultKind.Text:
                                        return Task.FromResult(new ScriptResponse()
                                        {
                                            Success = true,
                                            BeginTimestamp = beginTimestamp,
                                            EndTimestamp = endTimestamp,
                                            Kind = kind,
                                            Data = Convert.ToString(resultRawData),
                                        });

                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    throw new Exception();
                }

                return Task.FromResult(new ScriptResponse()
                {
                    Success = true,
                    BeginTimestamp = beginTimestamp,
                    EndTimestamp = endTimestamp,
                    Kind = ScriptResultKind.Text,
                    Data = Convert.ToString(resultObject),
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return Task.FromResult(new ScriptResponse()
                {
                    Success = false,
                    BeginTimestamp = beginTimestamp,
                    EndTimestamp = DateTime.UtcNow,
                    Exception = ex,
                });
            }
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
