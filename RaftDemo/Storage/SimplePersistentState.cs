﻿using DotNext;
using DotNext.Net.Cluster.Consensus.Raft.Commands;
using DotNext.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNext.IO;
using DotNext.Net.Cluster.Consensus.Raft;
using static DotNext.Threading.AtomicInt64;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace RaftDemo.Storage
{
    internal sealed class SimplePersistentState : MemoryBasedStateMachine, ISupplier<long>
    {
        internal const string LogLocation = "logLocation";

        private sealed class SimpleSnapshotBuilder : IncrementalSnapshotBuilder
        {
            private long value;

            public SimpleSnapshotBuilder(in SnapshotBuilderContext context)
                : base(context)
            {
            }

            protected override async ValueTask ApplyAsync(LogEntry entry)
                => value = await entry.ToTypeAsync<long, LogEntry>().ConfigureAwait(false);

            public override ValueTask WriteToAsync<TWriter>(TWriter writer, CancellationToken token)
                => writer.WriteAsync(value, token);
        }

        private long content;

        public SimplePersistentState(string path, AppEventSource source)
            : base(path, 50, CreateOptions(source))
        {
        }

        public SimplePersistentState(IConfiguration configuration, AppEventSource source)
            : this(configuration[LogLocation], source)
        {
        }

        private static Options CreateOptions(AppEventSource source)
        {
            var result = new Options
            {
                InitialPartitionSize = 50 * 8,
                WriteCounter = new("WAL.Writes", source),
                ReadCounter = new("WAL.Reads", source),
                CommitCounter = new("WAL.Commits", source),
                CompactionCounter = new("WAL.Compaction", source),
                LockContentionCounter = new("WAL.LockContention", source),
                LockDurationCounter = new("WAL.LockDuration", source),
            };

            result.WriteCounter.DisplayUnits =
                result.ReadCounter.DisplayUnits =
                result.CommitCounter.DisplayUnits =
                result.CompactionCounter.DisplayUnits = "entries";

            result.LockDurationCounter.DisplayUnits = "milliseconds";
            result.LockDurationCounter.DisplayName = "WAL Lock Duration";

            result.LockContentionCounter.DisplayName = "Lock Contention";

            result.WriteCounter.DisplayName = "Number of written entries";
            result.ReadCounter.DisplayName = "Number of retrieved entries";
            result.CommitCounter.DisplayName = "Number of committed entries";
            result.CompactionCounter.DisplayName = "Number of squashed entries";

            return result;
        }

        long ISupplier<long>.Invoke() => content.VolatileRead();

        private async ValueTask UpdateValue(LogEntry entry)
        {
            var value = await entry.ToTypeAsync<long, LogEntry>().ConfigureAwait(false);
            content.VolatileWrite(value);
            Console.WriteLine($"收到数据: {value}");
        }

        protected override ValueTask ApplyAsync(LogEntry entry)
            => entry.Length == 0L ? new ValueTask() : UpdateValue(entry);

        protected override SnapshotBuilder CreateSnapshotBuilder(in SnapshotBuilderContext context)
        {
            Console.WriteLine("正在生成快照");
            return new SimpleSnapshotBuilder(context);
        }
    }
}
