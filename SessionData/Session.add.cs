using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionData
{
    /// <summary>
    /// Session entity
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Required session update activity gain
        /// </summary>
        const int ACTIVITY_GRAIN_MSEC = 1000;
        /// <summary>
        /// Is session activity update required
        /// </summary>
        public bool IsActivityUpdateRequired
        {
            get
            {
                var ts = DateTime.Now;
                return (ACTIVITY_GRAIN_MSEC > ts.Subtract(LastActivity).TotalMilliseconds);
            }
        }

        /// <summary>
        /// Session Guid (string Id parse)
        /// </summary>
        public Guid SessionGuid
        {
            get
            {
                return Guid.Parse(Id);
            }
        }

        /// <summary>
        /// Debug support 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var res = "Session:";
            try
            {
                res += " Id=" + this.Id;
                res += "; User.Name=" + this.User.Name;
                res += "; User.Role.Count=" + this.User.Role.Count;
            }
            catch (Exception ex)
            {
                res += "ToStr Ex.:" + ex.ToString();
            }
            return res;
        }
    }
}
