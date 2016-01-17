using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionData
{
    /// <summary>
    /// Non cached (RAW) Session Manager
    /// </summary>
    public class SessionManager : ISessionManager
    {
        void ISessionManager.CloseSession(Guid session_guid)
        {
            DbManager.CloseSession(session_guid);
        }

        Session ISessionManager.CreateSession(long user_id)
        {
            return DbManager.CreateSession(user_id);
        }

        User ISessionManager.GetSessionUser(Guid session_guid)
        {
            var sess = DbManager.GetSession(session_guid, set_activity: true);
            return (sess == null ? null : sess.User); 
        }

        public static int GetDbSessionsCount()
        {
            return DbManager.GetDBSecssinsCount();
        }
    }
}
