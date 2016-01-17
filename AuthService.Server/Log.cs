using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Server
{
    /// <summary>
    /// Service logger (console output for debug only)
    /// </summary>
    static class Log
    {
        static readonly bool is_dbg_log = NFX.App.ConfigRoot.AttrByName("auth-service-dbg-log").ValueAsBool();
        static readonly bool is_info_log = NFX.App.ConfigRoot.AttrByName("auth-service-info-log").ValueAsBool();
        static readonly bool is_err_log = NFX.App.ConfigRoot.AttrByName("auth-service-err-log").ValueAsBool();

        public static void Info(string msg)
        {
            if (is_info_log)
            {
                Console.WriteLine(msg);
            }
        }

        public static void Dbg(string msg)
        {
            if (is_dbg_log)
            {
                Console.WriteLine(msg);
            }
        }

        public static void Error(string msg)
        {
            if (is_err_log)
            {
                Console.WriteLine(msg);
            }
        }
    }
}
