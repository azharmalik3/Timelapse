using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Entities;

namespace TestBed
{
    class Program
    {
        static void Main(string[] args)
        {
            TimelapseVideoInfo info = new TimelapseVideoInfo();

            string result = "  Duration: 00:00:00.20, start: 0.000000, bitrate: 879 kb/s, yuv420p, 640x480, q=2-31, 200 kb/s, 90k tbn, 5 tbc (default) frame=    1 fps=0.0 q=0.0 Lsize=N/A time=00:00:00.20 bitrate=N/A ";
            int index1 = result.IndexOf("Duration: ", StringComparison.Ordinal);
            int index2 = index1 + 8;
            if (index1 >= 0 && index2 >= 0)
                info.Duration = result.Substring(index1 + ("Duration: ").Length, index2 - index1);

            if (result.Contains("SAR"))
            {
                index2 = result.IndexOf("SAR", StringComparison.Ordinal) - 1;
                index1 = index2 - 10;
                info.Resolution = result.Substring(index1, index2 - index1).Trim();
            }
            else if (result.Contains("yuv420p"))
            {
                index1 = result.IndexOf("yuv420p, ", StringComparison.Ordinal) + ("yuv420p, ").Length;
                index2 = result.IndexOf(", ", index1);
                info.Resolution = result.Substring(index1, index2 - index1).Trim();
            }

            info.Resolution = info.Resolution.Replace(",", "");
            info.Resolution = info.Resolution.Replace(" ", "");

            index1 = result.LastIndexOf("frame=", StringComparison.Ordinal) + ("frame= ").Length;
            index2 = result.IndexOf("fps", index1, StringComparison.Ordinal) - 1;
            if (index1 >= 0 && index2 >= 0)
                info.SnapsCount = int.Parse(result.Substring(index1, index2 - index1).Trim());
        }
    }
}
