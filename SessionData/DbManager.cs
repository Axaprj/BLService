using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;

namespace SessionData
{
    /// <summary>
    /// DB routines
    /// </summary>
    public static class DbManager
    {
        /// <summary>
        /// Initialize DB for tests
        /// </summary>
        /// <param name="users_count">required users number</param>
        public static void InitializeDB(int users_count)
        {
            using (var dbx = new SessionsContainer())
            {
                var user_role = dbx.Role.Where(r => r.Id == UserRoles.User).First();
                var poweruser_role = dbx.Role.Where(r => r.Id == UserRoles.PowerUser).First();
                var admin_role = dbx.Role.Where(r => r.Id == UserRoles.Admin).First();
                users_count = users_count - dbx.User.Count();
                for (int inx = 0; inx < users_count; inx++)
                {
                    var usr = new User() { Name = "user" + Guid.NewGuid() };
                    usr.Role.Add(user_role);
                    if (inx % 2 == 0)
                        usr.Role.Add(poweruser_role);
                    if (inx % 3 == 0)
                        usr.Role.Add(admin_role);
                    dbx.User.Add(usr);
                }
                foreach (var sess in dbx.Session)
                {
                    dbx.Session.Remove(sess);
                }
                dbx.SaveChanges();
            }
        }

        /// <summary>
        /// Get session records count from db
        /// </summary>
        /// <returns></returns>
        public static int GetDBSecssinsCount()
        {
            using (var dbx = new SessionsContainer())
            {
                return dbx.Session.Count();
            }
        }

        /// <summary>
        /// Get Users
        /// </summary>
        /// <param name="count">requested users number</param>
        /// <returns></returns>
        public static User[] GetUsers(int count)
        {
            using (var dbx = new SessionsContainer())
            {
                return dbx.User.Take(count).ToArray();
            }
        }


        /// <summary>
        /// Close session
        /// </summary>
        /// <param name="session_guid"></param>
        public static void CloseSession(Guid session_guid)
        {
            using (var dbx = new SessionsContainer())
            {
                var sqlite_guid = session_guid.ToString();
                var sess = dbx.Session.Where(s => s.Id == sqlite_guid).FirstOrDefault();
                dbx.Session.Remove(sess);
                dbx.SaveChanges();
            }
        }

        /// <summary>
        /// Create session for user
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public static Session CreateSession(long user_id)
        {
            using (var dbx = new SessionsContainer())
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
                dbx.Session.Add(sess);
                dbx.SaveChanges();
                sess = dbx.Session
                    .Include(s => s.User.Role)
                    .Where(s => s.Id == sqlite_guid).First();
                return sess;
            }
        }

        /// <summary>
        /// Get session including user entity
        /// </summary>
        /// <param name="session_guid"></param>
        /// <param name="set_activity">update session activity flag</param>
        /// <returns></returns>
        public static Session GetSession(Guid session_guid, bool set_activity)
        {
            using (var dbx = new SessionsContainer())
            {
                var sqlite_guid = session_guid.ToString();
                var qry =
                    dbx.Session.Include(s => s.User.Role).Where(s => s.Id == sqlite_guid);
                var sess = qry.FirstOrDefault();
                if (set_activity && sess != null)
                {
                    if (sess.IsActivityUpdateRequired)
                    {
                        sess.LastActivity = DateTime.Now;
                        dbx.SaveChanges();
                    }
                }
                return sess;
            }
        }

        /// <summary>
        ///  Get all sessions including user entity
        /// </summary>
        /// <returns></returns>
        public static Session[] GetSessions()
        {
            using (var dbx = new SessionsContainer())
            {
                return dbx.Session.Include(s => s.User.Role).ToArray();
            }
        }

        /// <summary>
        /// Update sessions activity 
        /// if <see cref="Session.IsActivityUpdateRequired">required</see>
        /// </summary>
        /// <param name="sessions"></param>
        public static void UpdateActivity(Session[] sessions)
        {
            foreach (var sess in sessions)
            {
                if (sess.IsActivityUpdateRequired)
                {
                    try
                    {
                        using (var dbx = new SessionsContainer())
                        {
                            var sqlite_guid = sess.SessionGuid.ToString();
                            var db_sess = dbx.Session.Where(
                                s => s.Id == sqlite_guid).FirstOrDefault();
                            if (db_sess == null) continue;
                            db_sess.LastActivity = DateTime.Now;
                            dbx.SaveChanges();
                        }
                    }
                    catch (DbUpdateConcurrencyException cex)
                    {
                        // session was removed
                        var hres = cex.HResult;
                        Console.WriteLine("DbUpdateConcurrencyException on " + sess.ToString());
                    }

                }
            }
        }
    }
}

