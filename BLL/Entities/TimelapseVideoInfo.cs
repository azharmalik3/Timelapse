using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Entities
{
    public struct TimelapseVideoInfo
    {
        public int SnapsCount { get; set; }
        public long FileSize { get; set; }
        public string Duration { get; set; }
        public string Resolution { get; set; }
        public DateTime SnapshotDate { get; set; }
    }
}
