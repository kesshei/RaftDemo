using DotNext;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo
{
    internal sealed class FakeOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private sealed class ChangeToken : Disposable
        {

        }

        internal FakeOptionsMonitor(T value) => CurrentValue = value;

        public T CurrentValue { get; }

        T IOptionsMonitor<T>.Get(string name) => CurrentValue;

        IDisposable IOptionsMonitor<T>.OnChange(Action<T, string> listener) => new ChangeToken();
    }
}
