using DistributedAuthSystem.Constants;
using System.Collections.Generic;

namespace DistributedAuthSystem.Responses
{
    public class ThinSynchronizationRes
    {
        #region properties

        public string SenderId { get; set; }

        public ThinSynchroResult Type { get; set; }

        public long SynchroTimestamp { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }

        #endregion
    }
}