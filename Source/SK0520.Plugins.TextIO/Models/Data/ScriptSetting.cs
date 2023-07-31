using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models.Data
{
    public class ScriptSetting
    {
        #region property

        public string Name { get; set; } = string.Empty;

        public Dictionary<string, ScriptParameterKind> Parameters { get; set; } = new Dictionary<string, ScriptParameterKind>();

        public string Source { get; set; } = string.Empty;

        #endregion
    }
}
