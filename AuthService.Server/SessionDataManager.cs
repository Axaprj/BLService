using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Data.Entity;
using SessionData;

namespace AuthService.Server
{
    /// <summary>
    /// Session Data manager (based on a table in memory).
    /// DB processing via <see cref="StorageManager{TDbContainer}"/>
    /// with <see cref="SessionsContainer"/> DbContext handlers.
    /// Thread Safe.
    /// </summary>
    class SessionDataManager
    {
        static readonly object Instance_Lock = new object();
        static SessionDataManager _instance;

        readonly ReaderWriterLockSlim Data_Lock = new ReaderWriterLockSlim();
        readonly Dictionary<Guid, Session> Data = new Dictionary<Guid, Session>();
        readonly StorageManager<SessionsContainer> StorageManager;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static SessionDataManager Instance
        {
            get
            {
                lock (Instance_Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SessionDataManager();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// constructor with stored Sessions load 
        /// </summary>
        SessionDataManager()
        {
            StorageManager = new StorageManager<SessionsContainer>(this.handleSave, this.handleError);
            Data_Lock.EnterWriteLock();
            try
            {
                var sessions = StorageManager.Get<Session[]>((dbx) =>
                {
                    return dbx.Session.Include(s => s.User.Role).ToArray();
                });
                foreach (var sess in sessions)
                {
                    Data.Add(sess.SessionGuid, sess);
                }
            }
            finally
            {
                Data_Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// <see cref="StorageManager"/> save handler
        /// </summary>
        /// <param name="dbx"></param>
        void handleSave(SessionsContainer dbx)
        {
            try
            {
                dbx.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("(SAVE ERR) : \n" + ex.ToString());
            }
        }

        /// <summary>
        /// <see cref="StorageManager"/> errors handler
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="tag"></param>
        void handleError(Exception ex, object tag)
        {
            Log.Error("(" + tag + ") : \n" + ex.ToString());
        }

        /// <summary>
        /// BL: Close Session 
        /// </summary>
        /// <param name="session_guid"></param>
        public void CloseSession(Guid session_guid)
        {
            Data_Lock.EnterWriteLock();
            try
            {
                Data.Remove(session_guid);
            }
            finally
            {
                Data_Lock.ExitWriteLock();
            }
            StorageManager.UpdateAsync((dbx) =>
            {
                var sqlite_guid = session_guid.ToString();
                var sess = dbx.Session.Where(s => s.Id == sqlite_guid).First();
                dbx.Session.Remove(sess);
            }, "Close Session");
        }

        /// <summary>
        /// BL: Create Session 
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public Session CreateSession(long user_id)
        {
            var ts = DateTime.Now;
            var sqlite_guid = Guid.NewGuid().ToString();
            var sess = new Session()
            {
                Id = sqlite_guid,
                UserId = user_id,
                Created = ts,
                LastActivity = ts
            };
            // put request: session record create
            StorageManager.UpdateAsync((dbx) =>
            {
                dbx.Session.Add(sess);
            }, "Create Session");
            //put request: get created session with user and roles
            // (waiting for create because it in front of the requests queue)
            sess = StorageManager.Get<Session>((dbx) =>
            {
                return dbx.Session
                    .Include(s => s.User.Role)
                    .Where(s => s.Id == sqlite_guid).First();
            }, "Get New Session");
            Data_Lock.EnterWriteLock();
            try
            {
                Data.Add(sess.SessionGuid, sess);
            }
            finally
            {
                Data_Lock.ExitWriteLock();
            }
            return sess;
        }

        /// <summary>
        /// BL: Get Session (with User and Roles)  and update session activity timestamp
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        public Session GetSession(Guid session_guid)
        {
            Session sess = null;
            bool upd = false;
            Data_Lock.EnterReadLock();
            try
            {
                if (Data.ContainsKey(session_guid))
                {
                    sess = Data[session_guid];
                    if (sess.IsActivityUpdateRequired)
                    {
                        sess.LastActivity = DateTime.Now;
                        upd = true;
                    }
                }
            }
            finally
            {
                Data_Lock.ExitReadLock();
            }
            if (upd)
            {
                StorageManager.UpdateAsync((dbx) =>
                {
                    var db_sess = dbx.Session.Where(s => s.Id == sess.Id).First();
                    db_sess.LastActivity = sess.LastActivity;
                }, "Update Session Activity");
            }
            return sess;
        }
    }
}

