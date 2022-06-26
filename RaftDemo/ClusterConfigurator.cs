using DotNext.Net.Cluster;
using DotNext.Net.Cluster.Consensus.Raft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo
{
    internal sealed class ClusterConfigurator
    {
        internal static void LeaderChanged(ICluster cluster, IClusterMember? leader)
        {
            Debug.Assert(cluster is IRaftCluster);
            var term = ((IRaftCluster)cluster).Term;
            var timeout = ((IRaftCluster)cluster).ElectionTimeout;
            Console.WriteLine(leader is null
                ? "无法达成共识!"
                : $"选出新的集群领导。领导地址为 {leader.EndPoint}");
            Console.WriteLine($"本地群集成员的期限为 {term}. 选举超时{timeout}");
        }
    }
}
