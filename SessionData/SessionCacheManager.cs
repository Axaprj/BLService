using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SessionData
{
    /// <summary>
    /// Cached sessions manager. Static fields used.
    /// In ASP.NET may by useful use System.Web.Caching.Cache.
    /// </summary>
    public class SessionCacheManager : ISessionManager
    {
        static readonly object CacheLock = new object();
        static SessionCache _sessCache;

        static SessionCache SessCache
        {
            get
            {
                lock (CacheLock)
                {
                    if (_sessCache == null)
                    {
                        _sessCache = new SessionCache();
                    }
                    return _sessCache;
                }
            }
        }

        public static void TerminateCaching()
        {
            lock (CacheLock)
            {
                if (_sessCache != null)
                {
                    _sessCache.Dispose();
                    _sessCache = null;
                }
            }
        }

        void ISessionManager.CloseSession(Guid session_guid)
        {
            DbManager.CloseSession(session_guid);
            SessCache.RemoveSession(session_guid);
        }

        Session ISessionManager.CreateSession(long user_id)
        {
            var sess = DbManager.CreateSession(user_id);
            SessCache.AddSession(sess);
            return sess;
        }

        User ISessionManager.GetSessionUser(Guid session_guid)
        {
            var sess = SessCache.GetSession(session_guid);
            return (sess == null ? null : sess.User);
        }
    }
}
