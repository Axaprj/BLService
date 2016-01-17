using SessionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthListTest
{
    class AuthorizationCheckerTest
    {
        public bool IsSilent;
        public int ThreadIndex;
        public long UserId;
        public int IterationCount;
        public ISessionManager SessionMgr;

        public void DoTest()
        {
            var session_guid = SessionMgr.CreateSession(UserId).SessionGuid;
            for (int inx = 0; inx < IterationCount; inx++)
            {
                var user = SessionMgr.GetSessionUser(session_guid);
                if (user == null)
                {
                    throw new Exception("Session '" + session_guid + "' not found");
                }
                var user_role = user.Role.Where(r => r.Id == UserRoles.User).FirstOrDefault();
                if( user_role == null)
                {
                    throw new Exception("User '" + user.Name + "' has not User role");
                }
                var powerusr_role = user.Role.Where(r => r.Id == UserRoles.PowerUser).FirstOrDefault();
                var admin_role = user.Role.Where(r => r.Id == UserRoles.Admin).FirstOrDefault();
            }
            SessionMgr.CloseSession(session_guid);
        }
    }
}
