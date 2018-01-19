using DistributedAuthSystem.Models;
using System.Collections.Generic;

namespace DistributedAuthSystem.Requests
{
    public class FatSynchronizationReq
    {
        #region properties

        public string SenderId { get; set; }

        public Operation[] History { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }

        public long RequestTimestamp { get; set; }

        #endregion
    }
}