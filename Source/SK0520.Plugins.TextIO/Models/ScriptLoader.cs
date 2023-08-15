using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Logging;
using SK0520.Plugins.TextIO.Models.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models
{
    public class ScriptLoader
    {
        #region define

        private const string HashKind = "SHA512";

        #endregion

        public ScriptLoader(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<ScriptLoader>();
        }

        #region property

        private ILogger Logger { get; }

        #endregion

        #region function

        private string? GetRawOption(string name, IEnumerable<string> options)
        {
            var values = options
                .Select(a => a.Split('=', 2))
                .FirstOrDefault(a => a[0] == name)
            ;
            if(values is null)
            {
                return null;
            }
            if(string.IsNullOrWhiteSpace(values[1]))
            {
                return null;
            }

            return values[1].Trim();

        }

        private bool GetRequired(IEnumerable<string> options)
        {
            var value = GetRawOption("require", options);
            if(value is null)
            {
                return false;
            }

            return Convert.ToBoolean(value);
        }

        private string? GetDisplay(IEnumerable<string> options)
        {
            return GetRawOption("display", options);
        }

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

            var metaUpdateUri = string.Empty;
            var metaDebugHotReload = false;

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
                                var required = GetRequired(options.Skip(1));
                                var display = GetDisplay(options.Skip(1));
                                if (parameters.Any(a => a.Name == paramName))
                                {
                                    throw new Exception($"name: {paramName}");
                                }
                                parameters.Add(new ScriptParameter(
                                    paramName,
                                    display ?? paramName,
                                    required,
                                    Enum.Parse<ScriptParameterKind>(value, true)
                                ));
                            }
                            break;

                        case "update":
                            {
                                if(Uri.TryCreate(rawHeader.value, UriKind.Absolute, out var result))
                                {
                                    metaUpdateUri = result.ToString();
                                }
                            }
                            break;

                        case "debug-hot-reload":
                            {
                                metaDebugHotReload = Convert.ToBoolean(rawHeader.value);
                            }
                            break;

                        default:
                            Logger.LogWarning("ignore: {key} {value}", rawHeader.key, rawHeader.value);
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
            var hash = ComputeHash(source, HashKind);
            var timestamp = DateTime.UtcNow;
            var meta = new ScriptMetaSetting()
            {
                ScriptId = scriptId,
                CreatedTimestamp = timestamp,
                UpdatedTimestamp = timestamp,
                UpdateUri = metaUpdateUri,
                HashKind = hash.HashKind,
                HashValue = hash.HashValue,
                DebugHotReload = metaDebugHotReload,
            };

            return new ScriptSetting(head, meta, body);
        }

        public IHashData ComputeHash(string source, string hashKind = HashKind)
        {
            var binary = Encoding.UTF8.GetBytes(source);
            using var hashStream = new MemoryStream(binary);

            using var hashAlgorithm = SHA512.Create();

            var hashValue = hashAlgorithm.ComputeHash(hashStream);

            return new HashData(hashKind, hashValue);
        }

        #endregion
    }
}
