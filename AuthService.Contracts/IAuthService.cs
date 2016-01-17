using NFX.Glue;
using SessionData;
using System;

namespace AuthService.Contracts
{
    /// <summary>
    /// Sessions Manager Service (Authorization Server) contract
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface IAuthService 
    {
        /// <summary>
        /// Create session
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        Session CreateSession(long user_id);
        /// <summary>
        /// Get session user data if session exists
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        User GetSessionUser(Guid session_guid);
        /// <summary>
        /// Close session
        /// </summary>
        /// <param name="session_guid"></param>
        void CloseSession(Guid session_guid);
    }
}
