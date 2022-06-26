using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo.Storage
{
    internal sealed class AppEventSource : EventSource
    {
        public AppEventSource()
            : base("RaftDemo.Events")
        {
        }
    }
}
