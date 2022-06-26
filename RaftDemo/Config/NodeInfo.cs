using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftDemo
{
    internal class NodeInfo
    {
        /// <summary>
        /// 当前节点信息
        /// </summary>
        public string MainNode { get; set; }
        /// <summary>
        /// 全部节点信息
        /// </summary>
        public List<string> Nodes { get; set; }
    }
}
