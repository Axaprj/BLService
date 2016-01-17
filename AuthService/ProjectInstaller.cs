using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService
{
    /// <summary>
    /// Service installer
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
