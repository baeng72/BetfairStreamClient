using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests.ExchangeStream.Protocol
{
    /// <summary>
    /// Exception used by api to raise a status fail.
    /// </summary>
    public class StatusException : Exception
    {
        public readonly Model.StatusMessage.ErrorCodeEnum ErrorCode;
        public readonly string ErrorMessage;

        public StatusException(Model.StatusMessage message) : base(message.ErrorCode + ": " + message.ErrorMessage)
        {
            ErrorCode = (Model.StatusMessage.ErrorCodeEnum)message.ErrorCode;
            ErrorMessage = message.ErrorMessage;
        }
    }
}
