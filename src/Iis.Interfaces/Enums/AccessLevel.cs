using System.ComponentModel;

namespace Iis.Interfaces.Enums
{
    public enum AccessLevel
    { 
        [Description("НВ - Не визначено")]
        Undefined = 0,
        [Description("НТ - Не таємно")]
        Unclassified,
        [Description("ДСВ - Для службового використання")]
        Restricted,
        [Description("Т - Таємно")]
        Confidential,
        [Description("ЦТ - Цілком таємно")]
        Secret,
        [Description("ОС - Особливої важливості")]
        TopSecret
    }
}