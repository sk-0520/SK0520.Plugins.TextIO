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
            if (!source.StartsWith("/**", StringComparison.Ordinal))
            {
                throw new ArgumentException("/**");
            }

            var scriptDocTailIndex = source.IndexOf("*/");
            if (scriptDocTailIndex == -1)
            {
                throw new ArgumentException("*/");
            }

            var bodySource = source.Substring(scriptDocTailIndex + "*/".Length).Trim();
            if (string.IsNullOrEmpty(bodySource))
            {
                throw new Exception($"source");
            }

            var rawHeaders = source
                .Substring(0, scriptDocTailIndex)
                .Substring("/**".Length)
                .Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.TrimEntries)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Where(a => a.Contains('@'))
                .Select(a => new
                {
                    Index = a.IndexOf('@'),
                    Raw = a,
                })
                .Where(a => a.Index != -1)
                .Select(a => a.Raw.Substring(a.Index + 1))
                .Select(a => a.Trim())
                .Select(a => a.StartsWith("*") ? a.Substring(1) : a)
                .Select(a => a.Split(':', 2, StringSplitOptions.TrimEntries))
                .Select(a => (key: a[0], value: a[1].Trim()))
                .Where(a => a.value.Length != 0)
                .ToArray()
                ;

            string? name = null;

            var parameters = new List<ScriptParameter>();

            foreach (var rawHeader in rawHeaders)
            {
                if (name is null)
                {
                    if (rawHeader.key == "name")
                    {
                        name = rawHeader.value;
                    }
                }
                else
                {
                    switch (rawHeader.key)
                    {
                        case "parameters":
                            {
                                var optionValue = rawHeader.value.Split(':', 2, StringSplitOptions.TrimEntries);
                                var option = optionValue[0];
                                var value = optionValue[1];
                                var options = option.Split('#').Select(a => a.Trim()).ToArray();
                                var paramName = options[0];
                                var required = options[1] switch { "!" => true, "?" => false, _ => throw new NotImplementedException() };
                                if (parameters.Any(a => a.Name == paramName))
                                {
                                    throw new Exception($"name: {paramName}");
                                }
                                parameters.Add(new ScriptParameter(
                                    paramName,
                                    required,
                                    Enum.Parse<ScriptParameterKind>(value, true)
                                ));
                            }
                            break;
                    }
                }
            }

            if (name is null)
            {
                throw new Exception(nameof(name));
            }

            var scriptId = Guid.NewGuid();
            var head = new ScriptHeadSetting(scriptId, name, parameters);
            var body = new ScriptBodySetting(scriptId, bodySource);

            return new ScriptSetting(head, body);
        }

        #endregion
    }
}
