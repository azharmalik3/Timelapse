using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Entities
{
    public class Log
    {
        public int ID { get; set; }
        public int TimelapseId { get; set; }
        public string CameraId { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public int Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
