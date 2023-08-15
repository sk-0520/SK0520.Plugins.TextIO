using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models
{
    public class InjectionText : InjectionBase
    {
        public InjectionText(Guid scriptId, string scriptName, ILoggerFactory loggerFactory)
            : base(scriptId, scriptName, loggerFactory)
        { }

        #region function

        #endregion
    }
}
