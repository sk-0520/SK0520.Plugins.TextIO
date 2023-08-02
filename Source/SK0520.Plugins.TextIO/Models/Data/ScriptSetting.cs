using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models.Data
{
    public record ScriptHeadSetting(
        Guid ScriptId,
        string Name,
        List<ScriptParameter> Parameters
    );

    public record ScriptBodySetting(
        Guid ScriptId,
        string Source
    );

    public record ScriptSetting
    {
        public ScriptSetting(ScriptHeadSetting head, ScriptBodySetting body)
        {
            if (head.ScriptId != body.ScriptId)
            {
                throw new ArgumentException(nameof(body.ScriptId));
            }

            Head = head;
            Body = body;
        }

        public Guid ScriptId => Head.ScriptId;

        public ScriptHeadSetting Head { get; }
        public ScriptBodySetting Body { get; }
    }
}
