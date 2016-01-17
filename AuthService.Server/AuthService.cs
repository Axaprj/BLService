using System;
using SessionData;
using AuthService.Contracts;

namespace AuthService.Server
{
    /// <summary>
    /// Sessions Manager (Authorization Service) server implementation
    /// </summary>
    public class AuthService : IAuthService
    {
        /// <summary>
        /// <see cref="IAuthService.CloseSession(Guid)"/>
        /// </summary>
        /// <param name="session_guid"></param>
        public void CloseSession(Guid session_guid)
        {
            var msg = "CloseSession(session_guid:" + session_guid + ")";
            try
            {
                Log.Dbg(msg);
                SessionDataManager.Instance.CloseSession(session_guid);
            }
            catch (Exception ex)
            {
                Log.Error(msg + "Error:\n" + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// <see cref="IAuthService.CreateSession(long)"/>
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public Session CreateSession(long user_id)
        {
            var msg = "CreateSession(user_id:" + user_id + ")";
            try
            {
                Log.Dbg(msg);
                return SessionDataManager.Instance.CreateSession(user_id);
            }
            catch (Exception ex)
            {
                Log.Error(msg + "Error:\n" + ex.ToString());
                throw;
            }

        }

        /// <summary>
        /// <see cref="IAuthService.GetSessionUser(Guid)"/>
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        public User GetSessionUser(Guid session_guid)
        {
            var msg = "GetSessionUser(session_guid:" + session_guid + ")";
            try
            {
                Log.Dbg(msg);
                var sess = SessionDataManager.Instance.GetSession(session_guid);
                Log.Dbg("return " + (sess == null ? "null" : sess.ToString()) + ";");
                return (sess == null ? null : sess.User);
            }
            catch (Exception ex)
            {
                Log.Error(msg + "Error:\n" + ex.ToString());
                throw;
            }
        }
    }
}
