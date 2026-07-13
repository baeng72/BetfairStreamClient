using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFBot.Auth
{
    public class AppKeyAndSessionProvider
    {
        private string _appkey;
        private string _host;
        private string _password;
        private string _username;
        private string _certFile;

        private AppKeyAndSession? _session;

        public const string SSO_HOST_COM = "identitysso.betfair.com";
        public const string SSO_HOST_AU = "identitysso.betfair.com.au";
        public const string SSO_HOST_IT = "identitysso.betfair.it";
        public const string SSO_HOST_ES = "identitysso.betfair.es";

        public AppKeyAndSessionProvider(string ssoHost, string appkey, string username, string password, string certFile)
        {
            _host = ssoHost;
            _appkey = appkey;
            _username = username;
            _password = password;

            Timeout = TimeSpan.FromSeconds(30);
            //4hrs is normal expire time
            SessionExpireTime = TimeSpan.FromHours(3);
            _certFile = certFile;
            _session = null;
        }

        /// <summary>
        /// AppKey being used
        /// </summary>
        public string Appkey
        {
            get { return _appkey; }
        }

        /// <summary>
        /// Session expire time (default 3hrs)
        /// </summary>
        public TimeSpan SessionExpireTime { get; set; }

        /// <summary>
        /// Specifies the timeout
        /// </summary>
        public TimeSpan Timeout { get; set; }


        /// <summary>
        /// Constructs a new session token via identity SSO.
        /// Note: These are not cached.
        /// </summary>
        /// <exception cref="InvalidCredentialException">Thrown if authentication response is fail</exception>
        /// <exception cref="IOException">Thrown if authentication call fails</exception>
        /// <returns></returns>
        public AppKeyAndSession? GetOrCreateNewSession()
        {
            if (_session != null)
            {
                //have a cached session - is it expired
                if ((_session.CreateTime + SessionExpireTime) > DateTime.UtcNow)
                {
                    //Trace.TraceInformation("SSO Login - session not expired - re-using");
                    return _session;
                }
                else
                {
                    //Trace.TraceInformation("SSO Login - session expired");
                }
            }

            //Trace.TraceInformation("SSO Login host={0}, appkey={1}, username={2}",
            //    _host,
            //    _appkey,
            //    _username);
            LoginResponse? loginResponse = null;
            try
            {
                AuthClient authClient = new AuthClient(Appkey);
                loginResponse = authClient.doLogin(_username, _password, _certFile,_host);

              
            }
            catch (Exception e)
            {
                throw new IOException("SSO Authentication - call failed:", e);
            }

            //got a response - decode
            if (loginResponse != null)
            {
                _session = new AppKeyAndSession(_appkey, loginResponse.SessionToken??string.Empty, loginResponse.LoginStatus ?? string.Empty);

            }


            return _session;
        }

        /// <summary>
        /// Expires cached token
        /// </summary>
        public void ExpireTokenNow()
        {
            Trace.TraceInformation("SSO Login - expiring session token now");
            _session = null;
        }
    }

}
