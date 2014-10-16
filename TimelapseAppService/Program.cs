using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using BLL.Common;

namespace TimelapseAppService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

//#if(!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
                { 
                    new Service1()
                };
            ServiceBase.Run(ServicesToRun);
//#else
            //Service1 myServ = new Service1();
            //myServ.Start();
//#endif
        }

        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            if (e != null && e.ExceptionObject != null)
                Utils.FileLog("Unhandled Exception Occured..." + e.ExceptionObject.ToString());
        }
    }
}
