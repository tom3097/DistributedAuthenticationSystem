using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAuthSystem.Services
{
    public interface ISynchronizationsRepository
    {
        #region methods

        bool RegisterServer(string id);

        bool UnregisterServer(string id);

        bool UpdateTime(string id, long timestamp);

        bool GetSynchroTimesCopy(out Dictionary<string, long> synchroTimesCopy);

        void UpdateSynchroTimes(Dictionary<string, long> synchroTimesSource, string sourceId,
            long synchroToTime);

        void FixSynchroTimes(long maxSynchroTime);

        #endregion
    }
}
