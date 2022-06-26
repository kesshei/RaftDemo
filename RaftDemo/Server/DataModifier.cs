using DotNext;
using DotNext.Net.Cluster.Consensus.Raft;
using Microsoft.Extensions.Hosting;
using RaftDemo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaftDemo.Server
{
internal sealed class DataModifier : BackgroundService
{
    private readonly IRaftCluster cluster;
    private readonly ISupplier<long> valueProvider;

    public DataModifier(IRaftCluster cluster, ISupplier<long> provider)
    {
        this.cluster = cluster;
        valueProvider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);

            var leadershipToken = cluster.LeadershipToken;
            TitleInfo.Show(!leadershipToken.IsCancellationRequested);
            if (!leadershipToken.IsCancellationRequested)
            {
                var newValue = valueProvider.Invoke() + 500L;
                Console.WriteLine("保存领导节点生成的值 {0}", newValue);

                var source = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, leadershipToken);
                try
                {
                    var entry = new Int64LogEntry { Content = newValue, Term = cluster.Term };
                    await cluster.ReplicateAsync(entry, source.Token);
                }
                catch (Exception e)
                {
                    Console.WriteLine("未知异常 {0}", e);
                }
                finally
                {
                    source?.Dispose();
                }
            }
        }
    }
}
}
