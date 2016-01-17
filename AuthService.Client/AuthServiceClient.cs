using System;
using NFX.Glue;
using NFX.Glue.Protocol;
using AuthService.Contracts;
using SessionData;

namespace AuthService.Client
{
    /// <summary>
    ///  Sessions Manager (Authorization Service) client 
    /// (entered manually here; is generated usually)
    /// </summary>
    public class AuthServiceClient : ClientEndPoint, IAuthService
    {
        
        #region Static Members

        private static TypeSpec s_ts_CONTRACT;
        private static MethodSpec s_ms_CloseSession;
        private static MethodSpec s_ms_CreateSession;
        private static MethodSpec s_ms_GetSessionUser;

        //static .ctor
        static AuthServiceClient()
        {
            var t = typeof(IAuthService);
            s_ts_CONTRACT = new TypeSpec(t);
            s_ms_CloseSession = new MethodSpec(t.GetMethod("CloseSession", new Type[] { typeof(Guid) }));
            s_ms_CreateSession = new MethodSpec(t.GetMethod("CreateSession", new Type[] { typeof(long) }));
            s_ms_GetSessionUser = new MethodSpec(t.GetMethod("GetSessionUser", new Type[] { typeof(Guid) }));
        }
        #endregion

        #region .ctor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="binding"></param>
        public AuthServiceClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="binding"></param>
        public AuthServiceClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="glue"></param>
        /// <param name="node"></param>
        /// <param name="binding"></param>
        public AuthServiceClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="glue"></param>
        /// <param name="node"></param>
        /// <param name="binding"></param>
        public AuthServiceClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

        //common instance .ctor body
        private void ctor()
        {
        }
        #endregion

        /// <summary>
        /// Service contract type
        /// </summary>
        public override Type Contract
        {
            get { return typeof(IAuthService); }
        }

        /// <summary>
        /// <see cref="IAuthService.CloseSession(Guid)"/>
        /// </summary>
        /// <param name="session_guid"></param>
        public void CloseSession(Guid session_guid)
        {
            var call = Async_CloseSession(session_guid);
            call.CheckVoidValue();
        }

        /// <summary>
        /// Async call of <see cref="CloseSession(Guid)"/>
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        public CallSlot Async_CloseSession(Guid session_guid)
        {
            var request = new RequestAnyMsg(s_ts_CONTRACT, s_ms_CloseSession, false, RemoteInstance, new object[] { session_guid });
            return DispatchCall(request);
        }

        /// <summary>
        /// /// <see cref="IAuthService.CreateSession(long)"/>
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public Session CreateSession(long user_id)
        {
            var call = Async_CreateSession(user_id);
            return call.GetValue<Session>();
        }

        /// <summary>
        /// Async call of <see cref="CreateSession(long)"/>
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public CallSlot Async_CreateSession(long user_id)
        {
            var request = new RequestAnyMsg(s_ts_CONTRACT, s_ms_CreateSession, false, RemoteInstance, new object[] { user_id });
            return DispatchCall(request);
        }

        /// <summary>
        /// /// <see cref="IAuthService.GetSessionUser(Guid)"/>
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        public User GetSessionUser(Guid session_guid)
        {
            var call = Async_GetSessionUser(session_guid);
            return call.GetValue<User>();
        }

        /// <summary>
        /// Async call of <see cref="GetSessionUser(Guid)"/>
        /// </summary>
        /// <param name="session_guid"></param>
        /// <returns></returns>
        public CallSlot Async_GetSessionUser(Guid session_guid)
        {
            var request = new RequestAnyMsg(s_ts_CONTRACT, s_ms_GetSessionUser, false, RemoteInstance, new object[] { session_guid });
            return DispatchCall(request);
        }
    }
}


