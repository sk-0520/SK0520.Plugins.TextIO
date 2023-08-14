using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models
{
    public class InjectionBase
    {
        protected InjectionBase(Guid scriptId, string scriptName, ILoggerFactory loggerFactory)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger(GetType());
        }

        #region property

        public Guid ScriptId { get; }
        public string ScriptName { get; }
        protected ILogger Logger { get; }
        protected ILoggerFactory LoggerFactory { get; }

        #endregion
    }
}
