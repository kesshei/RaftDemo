
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo
{
    internal class TitleInfo
    {
        public static string Node { get; set; }
        public static void Show(bool IsLeader = false)
        {
            Console.Title = $"Raft Node : {Node} Leader : {IsLeader} Time : {DateTime.Now} by 蓝创精英团队";
        }
    }
}
