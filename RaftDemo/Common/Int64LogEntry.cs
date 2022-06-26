using DotNext.IO;
using DotNext.IO.Log;
using DotNext.Net.Cluster.Consensus.Raft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo.Common
{
    internal sealed class Int64LogEntry : BinaryTransferObject<long>, IRaftLogEntry
    {
        internal Int64LogEntry()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        bool ILogEntry.IsSnapshot => false;

        public long Term { get; set; }

        public DateTimeOffset Timestamp { get; }
    }
}
