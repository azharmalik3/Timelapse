using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Common
{
    public enum TimelapsePrivacy
    {
        Private = 0,
        Public = 1
    }

    public enum WatermarkPosition
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
    }

    public enum TimelapseStatus
    {
        New = 0,
        Processing = 1,
        Failed = 2,
        Scheduled = 3,
        Stopped = 4,
        NotFound = 5,
        Expired = 6
    }

    public enum TimelapseLogType
    {
        AppLog = 0,
        AppError = 1,
        RecorderLog = 2,
        RecorderError = 3
    }
}
