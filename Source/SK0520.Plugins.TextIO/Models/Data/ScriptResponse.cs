using ContentTypeTextNet.Pe.Bridge.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK0520.Plugins.TextIO.Models.Data
{
    /// <summary>
    /// 結果データ型。
    /// <para>実データの型ではなく出力の表示種別を意味する。</para>
    /// </summary>
    public enum ScriptResultKind
    {
        None,
        Text,
    }

    public record ScriptResponse
    {
        #region function

        [MemberNotNullWhen(true, nameof(Data))]
        [MemberNotNullWhen(false, nameof(Exception))]
        public required bool Success { get; init; }

        public ScriptResultKind Kind { get; init; }
        public object? Data { get; init; }

        public Exception? Exception { get; init; }

        [DateTimeKind(DateTimeKind.Utc)]
        public required DateTime BeginTimestamp { get; init; }
        [DateTimeKind(DateTimeKind.Utc)]
        public required DateTime EndTimestamp { get; init; }

        #endregion
    }
}
