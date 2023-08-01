using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models
{
    public class ScriptLoader
    {
        #region function

        public ScriptSetting LoadSource(string source)
        {
            if(!source.StartsWith("/**", StringComparison.Ordinal))
            {
                throw new ArgumentException("/**");
            }

            var scriptDocTailIndex = source.IndexOf("*/");
            if(scriptDocTailIndex == -1)
            {
                throw new ArgumentException("*/");
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}
