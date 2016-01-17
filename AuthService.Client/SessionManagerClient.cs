using SessionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Client
{
    /// <summary>
    /// Session Manager client (configuration reader and <see cref="AuthServiceClient"/> wrapper) 
    /// </summary>
    public class SessionManagerClient : ISessionManager
    {
        static string auth_service_node = 
            NFX.App.ConfigRoot.AttrByName("auth-service-node").Value;
        //"sync://localhost:8080";// "mpx://localhost:8081";

        /// <summary>
        /// <see cref="ISessionManager.CloseSession(Guid)"/> 
        /// </summary>
        /// <param name="session_guid"></param>
        public void CloseSession(Guid session_guid)
        {
            using (var c = new AuthServiceClient(auth_service_node))
            {

                c.CloseSession(session_guid);
            }
        }

        /// <summary>
        /// <see cref="ISessionManager.CreateSession(long)"/> 
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public Session CreateSession(long user_id)
        {
            using (var c = new AuthServiceClient(auth_service_node))
            {
                return c.CreateSession(user_id);
            }
        }

        /// <summary>
        /// <see cref="ISessionManager.GetSessionUser(Guid)"/> 
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        public User GetSessionUser(Guid session_guid)
        {
            using (var c = new AuthServiceClient(auth_service_node))
            {
                return c.GetSessionUser(session_guid);
            }
        }
    }
}
