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
        public ScriptParameterViewModelFactory(ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
        {
            SkeletonImplements = skeletonImplements;
            ContextDispatcher = contextDispatcher;
            LoggerFactory = loggerFactory;
        }

        #region property

        private ISkeletonImplements SkeletonImplements { get; }
        private IContextDispatcher ContextDispatcher { get; }
        private ILoggerFactory LoggerFactory { get; }

        #endregion

        #region function

        public ScriptParameterViewModelBase Create(ScriptParameter parameter)
        {
            return parameter.Kind switch
            {
                ScriptParameterKind.Boolean => new BooleanScriptParameterViewModel(parameter, SkeletonImplements, ContextDispatcher, LoggerFactory),
                ScriptParameterKind.String => new StringScriptParameterViewModel(parameter, SkeletonImplements, ContextDispatcher, LoggerFactory),
                ScriptParameterKind.Integer => new IntegerScriptParameterViewModel(parameter, SkeletonImplements, ContextDispatcher, LoggerFactory),
                ScriptParameterKind.Decimal => new DecimalScriptParameterViewModel(parameter, SkeletonImplements, ContextDispatcher, LoggerFactory),
                ScriptParameterKind.DateTime => new DateTimeScriptParameterViewModel(parameter, SkeletonImplements, ContextDispatcher, LoggerFactory),
                _ => throw new NotImplementedException(parameter.Kind.ToString()),
            };
        }

        #endregion
    }

    public abstract class ScriptParameterViewModelBase : ViewModelSkeleton
    {
        protected ScriptParameterViewModelBase(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
            : base(skeletonImplements, contextDispatcher, loggerFactory)
        {
            Parameter = parameter;
        }

        #region property

        protected ScriptParameter Parameter { get; }

        public string Name => Parameter.Name;
        public string Display => Parameter.Display;

        public bool IsRequired => Parameter.Required;

        public object? RawValue { get; protected set; }

        #endregion
    }

    public abstract class ScriptParameterViewModelBase<T> : ScriptParameterViewModelBase
    {
        #region variable

        [AllowNull]
        private T _value = default;

        #endregion

        protected ScriptParameterViewModelBase(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, contextDispatcher, loggerFactory)
        { }

        #region property

        [AllowNull]
        public T Value
        {
            get => this._value;
            set
            {
                SetProperty(ref this._value, value);
                RawValue = this._value;
            }
        }

        #endregion
    }

    public sealed class BooleanScriptParameterViewModel : ScriptParameterViewModelBase<bool?>
    {
        public BooleanScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, contextDispatcher, loggerFactory)
        { 
            Value = IsRequired ? false : null;
        }
    }
    public sealed class StringScriptParameterViewModel : ScriptParameterViewModelBase<string>
    {
        public StringScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, contextDispatcher, loggerFactory)
        {
            Value = IsRequired ? string.Empty : null;
        }
    }

    public sealed class IntegerScriptParameterViewModel : ScriptParameterViewModelBase<int?>
    {
        public IntegerScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, contextDispatcher, loggerFactory)
        { 
            Value = IsRequired ? 0 : null;
        }
    }

    public sealed class DecimalScriptParameterViewModel : ScriptParameterViewModelBase<decimal?>
    {
        public DecimalScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, contextDispatcher, loggerFactory)
        {
            Value = IsRequired ? 0 : null;
        }
    }
    public sealed class DateTimeScriptParameterViewModel : ScriptParameterViewModelBase<DateTime?>
    {
        public DateTimeScriptParameterViewModel(ScriptParameter parameter, ISkeletonImplements skeletonImplements, IContextDispatcher contextDispatcher, ILoggerFactory loggerFactory)
            : base(parameter, skeletonImplements, contextDispatcher, loggerFactory)
        {
            Value = DateTime.Now;
        }
    }
}
