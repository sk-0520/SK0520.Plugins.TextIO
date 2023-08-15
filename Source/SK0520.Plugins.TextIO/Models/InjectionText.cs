using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string[] splitLines(string? s)
        {
            if (s == null)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();
            using var reader = new StringReader(s);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                result.Add(line);
            }

            return result.ToArray();
        }

        #endregion
    }
}
