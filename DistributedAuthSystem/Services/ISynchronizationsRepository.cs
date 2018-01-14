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

        bool RegisterServer(int id);

        bool UnregisterServer(int id);

        bool UpdateSynchroTime(int id, long timestamp);

        bool GetSynchroTimesCopy(out Dictionary<int, long> synchroTimesCopy);

        #endregion
    }
}
