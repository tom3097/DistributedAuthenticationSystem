using DistributedAuthSystem.Constants;
using System.Collections.Generic;

namespace DistributedAuthSystem.Responses
{
    public class FatSynchronizationRes
    {
        #region properties

        public string SenderId { get; set; }

        public FatSynchroResult Type { get; set; }

        public long SynchroTimestamp { get; set; }

        public Dictionary<string, long> SynchroTimes { get; set; }

        public long RequestTimestamp { get; set; }

        #endregion
    }
}