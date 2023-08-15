using ContentTypeTextNet.Pe.Bridge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models.Data
{
    public interface IScriptId
    {
        #region property

        Guid ScriptId { get; }

        #endregion
    }

    public record ScriptHeadSetting(
        Guid ScriptId,
        string Name,
        List<ScriptParameter> Parameters
    ): IScriptId;

    public record ScriptMetaSetting: IHashData, IScriptId
    {
        #region property

        [DateTimeKind(DateTimeKind.Utc)]
        public required DateTime CreatedTimestamp { get; init; }

        [DateTimeKind(DateTimeKind.Utc)]
        public required DateTime UpdatedTimestamp { get; init; }

        public required string UpdateUri { get; init; }

        public bool DebugHotReload { get; init; } = false;

        #endregion

        #region IHash

        public required string HashKind { get; init; }

        public required byte[] HashValue { get; init; }

        #endregion

        #region IScriptId

        public required Guid ScriptId { get; init; }

        #endregion
    }

    public record ScriptBodySetting(
        Guid ScriptId,
        string Source
    ): IScriptId;

    public record ScriptSetting: IScriptId
    {
        public ScriptSetting(ScriptHeadSetting head, ScriptMetaSetting meta, ScriptBodySetting body)
        {
            if (head.ScriptId != body.ScriptId || head.ScriptId != meta.ScriptId)
            {
                throw new ArgumentException(nameof(body.ScriptId));
            }

            Head = head;
            Meta = meta;
            Body = body;
        }

        #region property

        public ScriptHeadSetting Head { get; }
        public ScriptMetaSetting Meta { get; }
        public ScriptBodySetting Body { get; }

        #endregion

        #region IScriptId

        public Guid ScriptId => Head.ScriptId;

        #endregion
    }
}
