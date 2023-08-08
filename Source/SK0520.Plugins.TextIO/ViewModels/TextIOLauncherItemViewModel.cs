using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.Plugin.Addon;
using ContentTypeTextNet.Pe.Bridge.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SK0520.Plugins.TextIO.Addon;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SK0520.Plugins.TextIO.ViewModels
{
    public class TextIOLauncherItemViewModel : ViewModelSkeleton
    {
        #region variable

        private bool _isRunning = false;

        private ScriptHeadViewModel? _scriptHead;

        private ICommand? _addScriptCommand;
        private ICommand? _updateScriptCommand;
        private ICommand? _removeScriptCommand;

        #endregion

        internal TextIOLauncherItemViewModel(TextIOLauncherItem item, ILauncherItemAddonContext launcherItemAddonContext, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            LoggerFactory = loggerFactory;
            Item = item;
            LauncherItemAddonContext = launcherItemAddonContext;
            var heads = Item.GetScriptHeads(LauncherItemAddonContext.Storage.Persistence);
            ScriptHeadCollection = new ObservableCollection<ScriptHeadViewModel>(heads.Select(a => new ScriptHeadViewModel(a, skeletonImplements, dispatcherWrapper, loggerFactory)));
        }

        #region property

        private ILoggerFactory LoggerFactory { get; }
        private TextIOLauncherItem Item { get; }
        private ILauncherItemAddonContext LauncherItemAddonContext { get; }

        public bool IsRunning
        {
            get => this._isRunning;
            set => SetProperty(ref this._isRunning, value);
        }

        public ObservableCollection<ScriptHeadViewModel> ScriptHeadCollection { get; }
        public ScriptHeadViewModel? SelectedScriptHead
        {
            get => this._scriptHead;
            set => SetProperty(ref this._scriptHead, value);
        }

        #endregion

        #region command

        public ICommand AddScriptCommand
        {
            get => this._addScriptCommand ??= CreateCommand(
                () =>
                {
                    try
                    {
                        var dialog = new OpenFileDialog()
                        {
                            DefaultExt = ".js",
                            Filter = "JavaScript (.js)|*.js",
                        };
                        if (dialog.ShowDialog().GetValueOrDefault())
                        {
                            var filePath = dialog.FileName;
                            var file = new FileInfo(filePath);
                            var script = Item.AddScriptFile(file);
                            ScriptHeadCollection.Add(new ScriptHeadViewModel(script.Head, Implements, DispatcherWrapper, LoggerFactory));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                        MessageBox.Show(ex.ToString());
                    }
                }
            );
        }

        public ICommand UpdateScriptCommand
        {
            get => this._updateScriptCommand ??= CreateCommand<ScriptHeadViewModel>(
                o =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            o.ScriptUpdateStatus = ScriptUpdateStatus.Success;
                            var meta = Item.GetMeta(o.ScriptId);
                            if(!Uri.TryCreate(meta.UpdateUri, UriKind.Absolute, out var uri))
                            {
                                o.ScriptUpdateStatus = ScriptUpdateStatus.None;
                                Logger.LogInformation("[{PLUGIN}:{SCRIPT}] アップデート対象なし", Item.LauncherItemId, o.ScriptId);
                                return;
                            }

                            var scriptSetting = await Item.UpdateScriptIfNewVersionAsync(meta, uri);
                            if(scriptSetting is null)
                            {
                                Logger.LogInformation("[{PLUGIN}:{SCRIPT}] アップデート対象なし", Item.LauncherItemId, o.ScriptId);
                                return;
                            }
                            Item.UpdateScript(scriptSetting);
                            var index = ScriptHeadCollection.IndexOf(o);
                            if(index!=-1)
                            {
                                await DispatcherWrapper.BeginAsync(() =>
                                {
                                    var newHead = new ScriptHeadViewModel(scriptSetting.Head, Implements, DispatcherWrapper, LoggerFactory);
                                    ScriptHeadCollection.RemoveAt(index);
                                    ScriptHeadCollection.Insert(index, newHead);
                                    SelectedScriptHead = newHead;
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            o.ScriptUpdateStatus = ScriptUpdateStatus.Failure;
                            Logger.LogError(ex, ex.Message);
                            MessageBox.Show(ex.ToString());
                        }
                    });
                },
                o => o is not null && o.ScriptUpdateStatus != ScriptUpdateStatus.Running
            );
        }

        public ICommand RemoveScriptCommand
        {
            get => this._removeScriptCommand ??= CreateCommand<ScriptHeadViewModel>(
                o =>
                {
                    try
                    {
                        Item.RemoveScript(o.ScriptId);
                        ScriptHeadCollection.Remove(o);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                        MessageBox.Show(ex.ToString());
                    }
                },
                o => o is not null
            );
        }

        #endregion

        #region function

        #endregion
    }
}
