using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.Auth
{
    /// <summary>
    /// Wraps an appkey & it's current session
    /// </summary>
    public class AppKeyAndSession
    {
        public AppKeyAndSession(string appkey, string token, string status)
        {
            AppKey = appkey;
            Token = token;
            CreateTime = DateTime.UtcNow;
            LoginStatus = status;
        }

        public string AppKey { get; private set; }
        public DateTime CreateTime { get; private set; }
        public string Token { get; private set; }

        public string LoginStatus { get; private set; }
    }
}
