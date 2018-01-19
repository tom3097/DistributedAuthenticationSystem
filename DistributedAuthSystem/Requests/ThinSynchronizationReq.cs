using System.Collections.Generic;

namespace DistributedAuthSystem.Requests
{
    public class ThinSynchronizationReq
    {
        #region properties

        public string SenderId { get; set; }

        public string LastHash { get; set; }

        public long SynchroTimestamp { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }

        public long RequestTimestamp { get; set; }

        #endregion
    }
}