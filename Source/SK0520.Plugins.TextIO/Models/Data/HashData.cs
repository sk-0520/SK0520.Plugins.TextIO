using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models.Data
{
    public interface IHashData
    {
        #region property

        string HashKind { get; }
        byte[] HashValue { get; }

        #endregion
    }

    public record HashData(
        string HashKind,
        byte[] HashValue
    ) : IHashData;
}
