// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "", Scope = "module")]
[assembly: SuppressMessage("Reliability", "CA1816:Change DistributionWorker.Dispose() to call GC.SuppressFinalize(object)", Justification = "", Scope = "module")]
[assembly: SuppressMessage("Reliability", "CA1052:Type 'Program' is a static holder type but is neither static nor NotInheritable", Justification = "", Scope = "module")]