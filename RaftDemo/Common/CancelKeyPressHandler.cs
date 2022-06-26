using DotNext.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo.Common
{
    internal sealed class CancelKeyPressHandler : AsyncManualResetEvent
    {
        internal CancelKeyPressHandler()
            : base(false)
        {
        }

        internal void Handler(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Set();
        }
    }
}
