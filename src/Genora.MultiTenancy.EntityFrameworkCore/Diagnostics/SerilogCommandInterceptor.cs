using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Genora.MultiTenancy.Diagnostics
{
    /// <summary>
    /// Ghi log query
    /// </summary>
    public class SerilogCommandInterceptor : DbCommandInterceptor
    {
        private const string SwKey = "__EF_SQL_SW";
        private static ILogger L => Log.ForContext("SourceContext", "EFCore.Sql");
        private static readonly ConcurrentDictionary<Guid, Stopwatch> _timers = new();

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        { PutSW(eventData); LogCmd("NonQueryExecuting", command, eventData); return base.NonQueryExecuting(command, eventData, result); }

        public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        { var ms = TakeMs(eventData); LogResult("NonQueryExecuted", command, eventData, ms, recordsAffected: result); return base.NonQueryExecuted(command, eventData, result); }

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
        { PutSW(eventData); LogCmd("NonQueryExecuting", command, eventData); return await base.NonQueryExecutingAsync(command, eventData, result, ct); }

        public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken ct = default)
        { var ms = TakeMs(eventData); LogResult("NonQueryExecuted", command, eventData, ms, recordsAffected: result); return await base.NonQueryExecutedAsync(command, eventData, result, ct); }

        public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        { PutSW(eventData); LogCmd("ScalarExecuting", command, eventData); return base.ScalarExecuting(command, eventData, result); }

        public override object ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object result)
        { var ms = TakeMs(eventData); LogResult("ScalarExecuted", command, eventData, ms, scalarResult: result); return base.ScalarExecuted(command, eventData, result); }

        public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken ct = default)
        { PutSW(eventData); LogCmd("ScalarExecuting", command, eventData); return await base.ScalarExecutingAsync(command, eventData, result, ct); }

        public override async ValueTask<object> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object result, CancellationToken ct = default)
        { var ms = TakeMs(eventData); LogResult("ScalarExecuted", command, eventData, ms, scalarResult: result); return await base.ScalarExecutedAsync(command, eventData, result, ct); }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        { PutSW(eventData); LogCmd("ReaderExecuting", command, eventData); return base.ReaderExecuting(command, eventData, result); }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        { var ms = TakeMs(eventData); LogResult("ReaderExecuted", command, eventData, ms); return base.ReaderExecuted(command, eventData, result); }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken ct = default)
        { PutSW(eventData); LogCmd("ReaderExecuting", command, eventData); return await base.ReaderExecutingAsync(command, eventData, result, ct); }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken ct = default)
        { var ms = TakeMs(eventData); LogResult("ReaderExecuted", command, eventData, ms); return await base.ReaderExecutedAsync(command, eventData, result, ct); }

        private static void LogCmd(string phase, DbCommand cmd, CommandEventData ed) =>
            L.Information("{Phase} {Db}@{DataSource}/{Database} Timeout={Timeout}s{NL}SQL:{NL}{Sql}{NL}Params:{@Params}",
                phase,
                ed.Command.Connection?.GetType().Name,
                ed.Command.Connection?.DataSource,
                ed.Command.Connection?.Database,
                ed.Command.CommandTimeout,
                Environment.NewLine,
                cmd.CommandText,
                cmd.Parameters.Cast<DbParameter>().Select(p => new { p.ParameterName, p.DbType, p.Value, p.Size, p.Direction })
            );

        private static void LogResult(string phase, DbCommand cmd, CommandExecutedEventData ed, long elapsedMs, int? recordsAffected = null, object scalarResult = null) =>
            L.Information("{Phase} ElapsedMs={Elapsed} RecordsAffected={RecordsAffected} Scalar={Scalar}{NL}SQL:{NL}{Sql}",
                phase, elapsedMs, recordsAffected, scalarResult, Environment.NewLine, cmd.CommandText);

        private static void PutSW(CommandEventData ed)
        {
            var sw = new Stopwatch();
            sw.Start();
            _timers[ed.CommandId] = sw;   // dùng CommandId để ghép cặp
        }

        private static long TakeMs(CommandExecutedEventData ed)
        {
            if (_timers.TryRemove(ed.CommandId, out var sw))
            {
                sw.Stop();
                return sw.ElapsedMilliseconds;
            }
            return -1;
        }
    }
}
