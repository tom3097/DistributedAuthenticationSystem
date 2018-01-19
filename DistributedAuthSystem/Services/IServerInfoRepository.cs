using DistributedAuthSystem.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAuthSystem.Services
{
    public interface IServerInfoRepository
    {
        #region methods

        string GetServerId();

        bool PutServerId(string id);

        ServerState GetServerState();

        void SetServerState(ServerState state);

        long GetLastFatSynchro();

        void SetLastFatSynchro(long lastFatSynchro);

        #endregion
    }
}
