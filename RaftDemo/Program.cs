using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNext.Net.Cluster;
using DotNext.Net.Cluster.Consensus.Raft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using RaftDemo.Common;
using RaftDemo.Server;
using RaftDemo.Storage;

namespace RaftDemo
{
class Program
{
    static async Task Main(string[] args)
    {
        await UseTcpTransport(Path.Combine(AppContext.BaseDirectory, "raftConfig"));
    }
    static Task UseTcpTransport(string path)
    {
        //获取所有配置
        var jsonConfiguration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        var NodeInfo = new NodeInfo();
        jsonConfiguration.Bind("NodeInfo", NodeInfo);
        Console.WriteLine($"MainNode:{NodeInfo.MainNode}");
        TitleInfo.Node = NodeInfo.MainNode;
        var configuration = new RaftCluster.TcpConfiguration(IPEndPoint.Parse(NodeInfo.MainNode))
        {
            RequestTimeout = TimeSpan.FromMilliseconds(140),
            LowerElectionTimeout = 150,
            UpperElectionTimeout = 300,
            TransmissionBlockSize = 4096,
            ColdStart = false,
        };

        //加载全部地址
        //线上环境自己重写服务
        var builder = configuration.UseInMemoryConfigurationStorage().CreateActiveConfigurationBuilder();
        foreach (var item in NodeInfo.Nodes)
        {
            var address = IPEndPoint.Parse(item);
            builder.Add(ClusterMemberId.FromEndPoint(address), address);
        }
        builder.Build();

        TitleInfo.Show();
        return UseConfiguration(configuration, path);
    }
    static async Task UseConfiguration(RaftCluster.NodeConfiguration config, string? persistentStorage)
    {
        var loggerFactory = new LoggerFactory();
        var loggerOptions = new ConsoleLoggerOptions
        {
            LogToStandardErrorThreshold = LogLevel.Warning
        };
        loggerFactory.AddProvider(new ConsoleLoggerProvider(new FakeOptionsMonitor<ConsoleLoggerOptions>(loggerOptions)));
        config.LoggerFactory = loggerFactory;
        using var cluster = new RaftCluster(config);
        cluster.LeaderChanged += ClusterConfigurator.LeaderChanged;
        var modifier = default(DataModifier?);
        if (!string.IsNullOrEmpty(persistentStorage))
        {
            var state = new SimplePersistentState(persistentStorage, new AppEventSource());
            cluster.AuditTrail = state;
            modifier = new DataModifier(cluster, state);
        }
        await cluster.StartAsync(CancellationToken.None);
        await (modifier?.StartAsync(CancellationToken.None) ?? Task.CompletedTask);
        //控制台等待取消
        using var handler = new CancelKeyPressHandler();
        Console.CancelKeyPress += handler.Handler;
        await handler.WaitAsync();
        Console.CancelKeyPress -= handler.Handler;

        //停止服务
        await (modifier?.StopAsync(CancellationToken.None) ?? Task.CompletedTask);
        await cluster.StopAsync(CancellationToken.None);
    }
}
}
