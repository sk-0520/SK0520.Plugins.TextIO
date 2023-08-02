using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.Plugin.Addon;
using ContentTypeTextNet.Pe.Bridge.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SK0520.Plugins.TextIO.Addon;
using System;
using System.Collections.Generic;
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

        private ICommand? _addScriptCommand;

        #endregion

        public TextIOLauncherItemViewModel(TextIOLauncherItem item, IContextWorker contextWorker, ILauncherItemAddonContext launcherItemAddonContext, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            Item = item;
            ContextWorker = contextWorker;
            LauncherItemAddonContext = launcherItemAddonContext;
        }

        #region property

        private TextIOLauncherItem Item { get; }
        private IContextWorker ContextWorker { get; }
        private ILauncherItemAddonContext LauncherItemAddonContext { get; }

        public bool IsRunning
        {
            get => this._isRunning;
            set => SetProperty(ref this._isRunning, value);
        }

        #endregion

        #region command

        public ICommand AddScriptCommand
        {
            get
            {
                return this._addScriptCommand ??= CreateCommand(
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
                                ContextWorker.RunPlugin(c =>
                                {
                                    Item.AddScriptFile(LauncherItemAddonContext.LauncherItemId, c.Storage.Persistence, file);
                                });
                            }
                        } catch (Exception ex) {
                            Logger.LogError(ex, ex.Message);
                            MessageBox.Show(ex.ToString());
                        }
                    }
                );
            }
        }

        #endregion
    }
}
