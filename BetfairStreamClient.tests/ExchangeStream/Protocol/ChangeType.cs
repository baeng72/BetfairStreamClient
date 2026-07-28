using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests.ExchangeStream.Protocol
{
    /// <summary>
    /// Common change type (as change type is local to market / order in swagger).
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Update
        /// </summary>
        UPDATE,
        /// <summary>
        /// Initial subscription image
        /// </summary>
        SUB_IMAGE,
        /// <summary>
        /// Resubscription delta image
        /// </summary>
        RESUB_DELTA,
        /// <summary>
        /// Heartbeat
        /// </summary>
        HEARTBEAT,
    }
}
