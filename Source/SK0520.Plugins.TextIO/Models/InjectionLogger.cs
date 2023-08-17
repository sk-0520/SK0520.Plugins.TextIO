using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models
{
    public class InjectionLogger : InjectionBase
    {
        public InjectionLogger(Guid scriptId, string scriptName, ILoggerFactory loggerFactory)
            : base(scriptId, scriptName, loggerFactory)
        { }

        #region property

        #endregion

        #region function

        public void info(object? message, params object?[] args)
        {
            if (args.Length == 0)
            {
                Logger.LogInformation("[{SCRIPT}] {obj}", ScriptId, message);
            }
            else
            {
                Logger.LogInformation("[{SCRIPT}] " + message, ScriptId, args);
            }
        }

        public void warn(object? message, params object?[] args)
        {
            if (args.Length == 0)
            {
                Logger.LogWarning("[{SCRIPT}] {obj}", ScriptId, message);
            }
            else
            {
                Logger.LogWarning("[{SCRIPT}] " + message, ScriptId, args);
            }
        }

        public void error(object? message, params object?[] args)
        {
            if (args.Length == 0)
            {
                Logger.LogError("[{SCRIPT}] {obj}", ScriptId, message);
            }
            else
            {
                Logger.LogError("[{SCRIPT}] " + message, ScriptId, args);
            }
        }

        public void dump(object? obj)
        {
            // JSON.stringify を噛ませたかったけどやり方わからん
            var strObj = ObjectDumper.Dump(obj);
            Logger.LogDebug("[{SCRIPT}] {OBJ}", ScriptId, strObj);
        }

        #endregion
    }
}
