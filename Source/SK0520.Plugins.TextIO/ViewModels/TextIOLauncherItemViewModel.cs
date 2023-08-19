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
using System.Windows.Documents;
using System.Windows.Input;

namespace SK0520.Plugins.TextIO.ViewModels
{
    public class TextIOLauncherItemViewModel : ViewModelSkeleton
    {
        #region variable

        private bool _isRunning = false;

        private string _inputValue = string.Empty;
        private string _outputValue = string.Empty;
        private bool _outputIsError = false;
        private bool _isVisibleProperty = false;

        private ScriptHeadViewModel? _scriptHead;

        private ICommand? _inputAllPasteCommand;
        private ICommand? _addScriptCommand;
        private ICommand? _updateScriptCommand;
        private ICommand? _moveUpScriptCommand;
        private ICommand? _moveDownScriptCommand;
        private ICommand? _removeScriptCommand;
        private ICommand? _executeCommand;
        private ICommand? _showPropertyCommand;
        private ICommand? _hidePropertyCommand;
        private ICommand? _outputAllCopyCommand;

        #endregion

        internal TextIOLauncherItemViewModel(TextIOLauncherItem item, ILauncherItemAddonContext launcherItemAddonContext, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            Item = item;
            LauncherItemAddonContext = launcherItemAddonContext;
            var heads = Item.GetScriptHeads(LauncherItemAddonContext.Storage.Persistence);
            ScriptHeadCollection = new ObservableCollection<ScriptHeadViewModel>(heads.Select(a => new ScriptHeadViewModel(a, skeletonImplements, dispatcherWrapper, loggerFactory)));
        }

        #region property

        private TextIOLauncherItem Item { get; }
        private ILauncherItemAddonContext LauncherItemAddonContext { get; }

        public bool IsRunning
        {
            get => this._isRunning;
            set => SetProperty(ref this._isRunning, value);
        }

        public string InputValue
        {
            get => this._inputValue;
            set => SetProperty(ref this._inputValue, value);
        }
        public string OutputValue
        {
            get => this._outputValue;
            set => SetProperty(ref this._outputValue, value);
        }

        public bool OutputIsError
        {
            get => this._outputIsError;
            set => SetProperty(ref this._outputIsError, value);
        }

        public bool IsVisibleProperty
        {
            get => this._isVisibleProperty;
            private set => SetProperty(ref this._isVisibleProperty, value);
        }

        public ObservableCollection<ScriptHeadViewModel> ScriptHeadCollection { get; }
        public ScriptHeadViewModel? SelectedScriptHead
        {
            get => this._scriptHead;
            set
            {
                SetProperty(ref this._scriptHead, value);
                OnPropertyChanged(nameof(IsSelectedScriptHead));
            }
        }

        public bool IsSelectedScriptHead => SelectedScriptHead is not null;

        #endregion

        #region command

        public ICommand InputAllPasteCommand => this._inputAllPasteCommand ??= CreateCommand(
            () =>
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    InputValue = text;
                }
            }
        );

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
                        OutputIsError = true;
                        OutputValue = ex.ToString();
                    }
                }
            );
        }

        public ICommand UpdateScriptCommand => this._updateScriptCommand ??= CreateCommand<ScriptHeadViewModel>(
            o =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        o.ScriptUpdateStatus = ScriptUpdateStatus.Running;

                        var meta = Item.GetMeta(o.ScriptId);
                        if (!Uri.TryCreate(meta.UpdateUri, UriKind.Absolute, out var uri))
                        {
                            o.ScriptUpdateStatus = ScriptUpdateStatus.None;
                            Logger.LogInformation("[{PLUGIN}:{SCRIPT}] アップデート対象なし", Item.LauncherItemId, o.ScriptId);
                            return;
                        }

                        var scriptSetting = await Item.UpdateScriptIfNewVersionAsync(meta, uri);
                        if (scriptSetting is null)
                        {
                            o.ScriptUpdateStatus = ScriptUpdateStatus.None;
                            Logger.LogInformation("[{PLUGIN}:{SCRIPT}] アップデート対象なし", Item.LauncherItemId, o.ScriptId);
                            return;
                        }
                        Item.UpdateScript(scriptSetting);
                        var index = ScriptHeadCollection.IndexOf(o);
                        if (index != -1)
                        {
                            await DispatcherWrapper.BeginAsync(() =>
                            {
                                var newHead = new ScriptHeadViewModel(scriptSetting.Head, Implements, DispatcherWrapper, LoggerFactory);
                                ScriptHeadCollection.RemoveAt(index);
                                ScriptHeadCollection.Insert(index, newHead);
                                SelectedScriptHead = newHead;
                                newHead.ScriptUpdateStatus = ScriptUpdateStatus.Success;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        o.ScriptUpdateStatus = ScriptUpdateStatus.Failure;
                        Logger.LogError(ex, ex.Message);
                        OutputIsError = true;
                        OutputValue = o.Name + Environment.NewLine + "--------------" + Environment.NewLine + ex.ToString();
                    }
                });
            },
            o => o is not null && o.ScriptUpdateStatus != ScriptUpdateStatus.Running
        );

        public ICommand MoveUpScriptCommand => this._moveUpScriptCommand ??= CreateCommand<ScriptHeadViewModel>(
            o => MoveScript(true, o),
            o => o is not null && ScriptHeadCollection.IndexOf(o) != 0
        );

        public ICommand MoveDownScriptCommand => this._moveDownScriptCommand ??= CreateCommand<ScriptHeadViewModel>(
            o => MoveScript(false, o),
            o => o is not null && ScriptHeadCollection.IndexOf(o) != ScriptHeadCollection.Count - 1
        );

        public ICommand RemoveScriptCommand => this._removeScriptCommand ??= CreateCommand<ScriptHeadViewModel>(
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

        public ICommand ExecuteCommand => this._executeCommand ??= CreateCommand(
            () =>
            {
                if (SelectedScriptHead is null)
                {
                    Logger.LogDebug("対象スクリプト未選択");
                    return;
                }
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var meta = Item.GetMeta(SelectedScriptHead.ScriptId);
                        if (meta.DebugHotReload && Uri.TryCreate(meta.UpdateUri, UriKind.Absolute, out var updateUri))
                        {
                            Logger.LogInformation("[{SCRIPT}] デバッグアップデート確認 {updateUri}", SelectedScriptHead.ScriptId, updateUri);
                            var scriptSetting = await Item.UpdateScriptIfNewVersionAsync(meta, updateUri);
                            if (scriptSetting is not null)
                            {
                                Logger.LogInformation("[{SCRIPT}] デバッグアップデート実行", SelectedScriptHead.ScriptId);
                                Item.UpdateScript(scriptSetting);
                            }
                        }

                        var options = SelectedScriptHead.ParameterCollection
                            .Select(a =>
                            {
                                if (a.IsRequired && a.RawValue is null)
                                {
                                    throw new InvalidDataException(a.Display);
                                }
                                return a;
                            })
                            .ToDictionary(k => k.Name, v => v.RawValue)
                        ;
                        Logger.LogInformation("[{SCRIPT}] 実行 {options}", SelectedScriptHead.ScriptId, options);
                        var result = await Item.RunScriptAsync(SelectedScriptHead.ScriptId, InputValue, options);
                        Logger.LogInformation("[{SCRIPT}] <{SUCCESS}> {TIME} - {KIND}: {DATA}", SelectedScriptHead.ScriptId, result.Success ? "Success" : "Failure", result.EndTimestamp - result.BeginTimestamp, result.Kind, result.Data);
                        if (result.Success)
                        {
                            switch (result.Kind)
                            {
                                case ScriptResultKind.Text:
                                    OutputValue = Item.ConvertString(result.Data);
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                            OutputIsError = false;
                        }
                        else
                        {
                            OutputIsError = true;
                            OutputValue = SelectedScriptHead.Name + Environment.NewLine + "--------------" + Environment.NewLine + result.Exception.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                        OutputIsError = true;
                        OutputValue = SelectedScriptHead.Name + Environment.NewLine + "--------------" + Environment.NewLine + ex.ToString();
                        return;
                    }
                });
            }
        );

        public ICommand ShowPropertyCommand => this._showPropertyCommand ??= CreateCommand(
            () =>
            {
                if (SelectedScriptHead is null)
                {
                    Logger.LogDebug("対象スクリプト未選択");
                    return;
                }

                var meta = Item.GetMeta(SelectedScriptHead.ScriptId);

                IsVisibleProperty = true;
            }
        );

        public ICommand HidePropertyCommand => this._hidePropertyCommand ??= CreateCommand(
            () =>
            {
                IsVisibleProperty = false;
            }
        );

        public ICommand OutputAllCopyCommand => this._outputAllCopyCommand ??= CreateCommand(
            () =>
            {
                if (!string.IsNullOrEmpty(OutputValue))
                {
                    Clipboard.SetText(OutputValue);
                }
            }
        );

        #endregion

        #region function

        private void MoveScript(bool isUp, ScriptHeadViewModel head)
        {
            var index = ScriptHeadCollection.IndexOf(head);
            Item.ChangeOrder(head.ScriptId, isUp);

            ScriptHeadCollection.RemoveAt(index);
            if (isUp)
            {
                ScriptHeadCollection.Insert(index - 1, head);
            }
            else
            {
                ScriptHeadCollection.Insert(index + 1, head);
            }
            SelectedScriptHead = head;
        }

        #endregion
    }
}
