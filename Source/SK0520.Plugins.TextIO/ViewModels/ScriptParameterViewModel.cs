using ContentTypeTextNet.Pe.Bridge.Models;
using ContentTypeTextNet.Pe.Bridge.ViewModels;
using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.ViewModels
{
    public class ScriptParameterViewModelFactory
    {
        public ScriptParameterViewModelFactory(ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
        {
            SkeletonImplements = skeletonImplements;
            DispatcherWrapper = dispatcherWrapper;
            LoggerFactory = loggerFactory;
        }

        #region property

        private ISkeletonImplements SkeletonImplements { get; }
        private IDispatcherWrapper DispatcherWrapper { get; }
        private ILoggerFactory LoggerFactory { get; }

        #endregion

        #region function

        public ScriptParameterViewModel Create(ScriptParameter parameter)
        {
            return parameter.Kind switch
            {
                ScriptParameterKind.String => new StringScriptParameterViewModel(parameter, SkeletonImplements, DispatcherWrapper, LoggerFactory),
                ScriptParameterKind.Integer => new IntegerScriptParameterViewModel(parameter, SkeletonImplements, DispatcherWrapper, LoggerFactory),
                ScriptParameterKind.Decimal => new DecimalScriptParameterViewModel(parameter, SkeletonImplements, DispatcherWrapper, LoggerFactory),
                ScriptParameterKind.DateTime => new DateTimeScriptParameterViewModel(parameter, SkeletonImplements, DispatcherWrapper, LoggerFactory),
                _ => throw new NotImplementedException(parameter.Kind.ToString()),
            };
        }

        #endregion
    }

    public abstract class ScriptParameterViewModel : ViewModelSkeleton
    {
        protected ScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(skeletonImplements, dispatcherWrapper, loggerFactory)
        {
            Parameter = parameter;
        }

        #region property

        protected ScriptParameter Parameter { get; }

        public bool IsRequired => Parameter.Required;

        #endregion
    }

    public abstract class ScriptParameterViewModelBase<T> : ScriptParameterViewModel
    {
        #region variable

        [AllowNull]
        private T _value = default;

        #endregion

        protected ScriptParameterViewModelBase(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, dispatcherWrapper, loggerFactory)
        { }

        #region property

        public T Value
        {
            get => this._value;
            set => SetProperty(ref this._value, value);
        }

        #endregion
    }

    public sealed class StringScriptParameterViewModel : ScriptParameterViewModel
    {
        public StringScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, dispatcherWrapper, loggerFactory)
        { }
    }
    public sealed class IntegerScriptParameterViewModel : ScriptParameterViewModel
    {
        public IntegerScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, dispatcherWrapper, loggerFactory)
        { }
    }

    public sealed class DecimalScriptParameterViewModel : ScriptParameterViewModel
    {
        public DecimalScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, dispatcherWrapper, loggerFactory)
        { }
    }
    public sealed class DateTimeScriptParameterViewModel : ScriptParameterViewModel
    {
        public DateTimeScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IDispatcherWrapper dispatcherWrapper, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, dispatcherWrapper, loggerFactory)
        { }
    }
}
