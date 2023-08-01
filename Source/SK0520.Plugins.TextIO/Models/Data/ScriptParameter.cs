using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models.Data
{
    public record ScriptParameter(
        string Name,
        bool Required,
        ScriptParameterKind Kind
    );
}
