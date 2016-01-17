using AuthService.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AuthService
{
    /// <summary>
    /// Service main
    /// </summary>
    public partial class ServiceMain : ServiceBase
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ServiceMain()
        {
            InitializeComponent();
        }

        ServerFactory Server = new ServerFactory();
        /// <summary>
        /// On Start custom handler
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Start Service");
            Server.StartServer(args);
        }

        /// <summary>
        /// On Stop custom handler
        /// </summary>
        protected override void OnStop()
        {
            Console.WriteLine("Stop Service");
            Server.StopServer();
        }

        /// <summary>
        /// On Shutdown custom handler
        /// </summary>
        protected override void OnShutdown()
        {
            Server.Dispose();
            base.OnShutdown();
        }

        /// <summary>
        /// Console test runner 
        /// </summary>
        /// <param name="args"></param>
        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
            this.OnStop();
        }
    }
}
