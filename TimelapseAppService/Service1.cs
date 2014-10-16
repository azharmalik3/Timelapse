using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BLL.Common;

namespace TimelapseAppService
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        static Thread main = null;
        static Executor executor = new Executor();

        public Service1()
        {
            InitializeComponent();
        }

        public void Start()
        {
            Utils.FileLog("Starting TimelapseAppService [debug mode]...");
            executor.Execute();
            try
            {
                Utils.FileLog("Starting TimelapseAppService... (Start)");
                timer.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
                timer.Interval = 5000;
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception e)
            {
                Utils.FileLog("Error TimelapseAppService..." + e.ToString());
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Utils.FileLog("Starting TimelapseAppService... (OnStart)");
                timer.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
                timer.Interval = 5000;
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception e)
            {
                Utils.FileLog("Error TimelapseAppService..." + e.ToString());
                throw;
            }
        }

        private void timer1_Elapsed(object sender, EventArgs e)
        {
            Utils.FileLog("TimelapseAppService Timer Elapsed...");
            timer.Enabled = false;
            timer.Stop();
            main = new Thread(new ThreadStart(executor.Execute));
            main.Start();
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
            timer.Stop();
            executor.StopExecution();
            Utils.FileLog("...Stopping TimelapseAppService");
        }
    }
}
