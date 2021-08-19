// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "", Scope = "module")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Leads to ObjectDisposedException", Scope = "member", Target = "~M:Iis.Utility.Logging.LoggingMiddleware.FormatResponse(Microsoft.AspNetCore.Http.HttpContext,System.Diagnostics.Stopwatch)~System.Threading.Tasks.Task{System.String}")]
