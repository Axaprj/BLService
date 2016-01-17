using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SessionData
{
    /// <summary>
    /// Cache of session data
    /// </summary>
    public class SessionCache : IDisposable
    {
        /// <summary>
        /// Required refresh period
        /// </summary>
        const int REFRESH_TIMEOUT_MSEC = 1000;

        readonly ReaderWriterLockSlim Cache_Lock = new ReaderWriterLockSlim();
        readonly Dictionary<Guid, Session> Cache = new Dictionary<Guid, Session>();
        readonly Thread CacheThread;
        DateTime RefreshTS = DateTime.MinValue;
        long disposedValue = 0;

        public SessionCache()
        {
            CacheThread = new Thread(new ThreadStart(handlerCacheRefresh));
            CacheThread.Start();
        }

        /// <summary>
        /// Endless refresh loop of session data (thread) 
        /// </summary>
        void handlerCacheRefresh()
        {
            while (Interlocked.Read(ref disposedValue) == 0)
            {
                // calculation of allowed wait 
                var obsolescence_msec = DateTime.Now.Subtract(RefreshTS).TotalMilliseconds;
                if(obsolescence_msec < REFRESH_TIMEOUT_MSEC)
                {
                    Thread.Sleep((int)(REFRESH_TIMEOUT_MSEC - obsolescence_msec));
                }
                // update cached sessions activity
                Session[] sessions;
                Cache_Lock.EnterReadLock();
                try
                {
                    sessions = Cache.Values.ToArray();
                }
                finally
                {
                    Cache_Lock.ExitReadLock();
                }
                DbManager.UpdateActivity(sessions);
                // load from db all sessions in the cache
                sessions = DbManager.GetSessions();
                RefreshTS = DateTime.Now;
                Cache_Lock.EnterWriteLock();
                try
                {
                    Cache.Clear();
                    foreach (var sess in sessions)
                        Cache.Add(sess.SessionGuid, sess);
                }
                finally
                {
                    Cache_Lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Add session to cache
        /// </summary>
        /// <param name="sess"></param>
        public void AddSession(Session sess)
        {
            Cache_Lock.EnterWriteLock();
            try
            {
                if (!Cache.ContainsKey(sess.SessionGuid))
                {
                    Cache.Add(sess.SessionGuid, sess);
                }
            }
            finally
            {
                Cache_Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Remove sessin from cache
        /// </summary>
        /// <param name="session_guid"></param>
        public void RemoveSession(Guid session_guid)
        {
            Cache_Lock.EnterWriteLock();
            try
            {
                if (Cache.ContainsKey(session_guid))
                {
                    Cache.Remove(session_guid);
                }
            }
            finally
            {
                Cache_Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get session from cache. If not found try to load from DB.
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        public Session GetSession(Guid session_guid)
        {
            Cache_Lock.EnterUpgradeableReadLock();
            try
            {
                if (Cache.ContainsKey(session_guid))
                {
                    return Cache[session_guid];
                }
                else
                {
                    var sess = DbManager.GetSession(session_guid, set_activity: false);
                    if (sess != null)
                    {
                        Cache_Lock.EnterWriteLock();
                        try
                        {
                            Cache.Add(session_guid, sess);
                        }
                        finally
                        {
                            Cache_Lock.ExitWriteLock();
                        }
                    }
                    return sess;
                }
            }
            finally
            {
                Cache_Lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Dispose with thread stop
        /// </summary>
        public void Dispose()
        {
            Interlocked.Increment(ref disposedValue);
        }
    }
}
