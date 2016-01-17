using NFX;
using NFX.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Server
{
    /// <summary>
    /// Server instances factory (NFX.Glue here)
    /// </summary>
    public class ServerFactory : IDisposable
    {
        /// <summary>
        /// Provides base implementation of IApplication for applications 
        /// that have no forms like services and console apps. 
        /// This class IS thread safe
        /// </summary>
        ServiceBaseApplication _application;
        readonly object ApplicationLock = new object();

        /// <summary>
        /// Start servers
        /// </summary>
        /// <param name="args">run arguments (not used now)</param>
        public void StartServer(string[] args)
        {
            lock (ApplicationLock)
            {
                if (_application == null)
                {
                    // NFX app base 
                    var nfx_arg = new string[] { "/config", "AuthService.Server.laconf" };
                    _application = new ServiceBaseApplication(
                            args: nfx_arg, rootConfig: null);
                    Log.Info("server is running...");
                    Log.Info("Glue servers:");
                    foreach (var service in App.Glue.Servers)
                        Log.Info("   " + service);
                }
            }
        }
                
        /// <summary>
        /// Stop servers
        /// </summary>
        public void StopServer()
        {
            Log.Info("server is stoping...");
            ServiceBaseApplication app;
            lock (ApplicationLock)
            {
                app = _application;
                _application = null;
            }
            if (app == null)
            {
                Log.Info("server is already stoped.");
            }
            else
            {
                app.Dispose();
                Log.Info("server is stoped.");
            }
        }

        #region IDisposable Support
        private volatile bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose with <see cref="StopServer"/>
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                    }
                    StopServer();
                    disposedValue = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        /// <summary>
        /// Destructor with dispose
        /// </summary>
        ~ServerFactory()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern. 
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
