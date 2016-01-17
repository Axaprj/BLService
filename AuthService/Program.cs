using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AuthService
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// Dual Service/Console.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                ServiceMain service1 = new ServiceMain();
                service1.TestStartupAndStop(args);
            }
            else
            {
                // set the current directory to the directory that service is running from 
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new ServiceMain()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
