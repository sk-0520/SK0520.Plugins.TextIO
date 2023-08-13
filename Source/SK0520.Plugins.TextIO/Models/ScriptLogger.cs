using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models
{
    public class ScriptLogger
    {
        public ScriptLogger(Guid scriptId, string scriptName, ILoggerFactory loggerFactory)
        {
            ScriptId = scriptId;
            ScriptName = scriptName;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        #region property

        private Guid ScriptId { get; }
        private string ScriptName { get; }

        private ILogger Logger { get; }

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
            Logger.LogDebug("[{SCRIPT}] {OBJ}", ScriptId, obj);
        }

        #endregion
    }
}
